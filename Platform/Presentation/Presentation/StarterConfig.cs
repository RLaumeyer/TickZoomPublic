﻿#region Header

/*
 * Software: TickZoom Trading Platform
 * Copyright 2009 M. Wayne Walter
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Business use restricted to 30 days except as otherwise stated in
 * in your Service Level Agreement (SLA).
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, see <http://www.tickzoom.org/wiki/Licenses>
 * or write to Free Software Foundation, Inc., 51 Franklin Street,
 * Fifth Floor, Boston, MA  02110-1301, USA.
 *
 */

#endregion Header

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Media;
using TickZoom.Common;
using System.Threading;
using TickZoom.Api;
using TickZoom.Presentation.Framework;

namespace TickZoom.Presentation
{
    public class StarterConfig : AutoBindable, IDisposable
    {
        #region Fields

        private readonly BackgroundWorker commandWorker;
        private readonly Log log;
        private readonly Dictionary<int, Progress> progressChildren = new Dictionary<int, Progress>();
        private readonly ConfigFile projectConfig;
        private string alarmFile;
        private int breakAtBar;
        private BarUnit chartBarUnit = BarUnit.Hour;
        private int chartPeriod = 1;
        private ushort servicePort;
        private ModelLoaderInterface loaderInstance;
        private string dataSubFolder;
        
		public string DataSubFolder {
			get { return dataSubFolder; }
			set { NotifyOfPropertyChange( () => DataSubFolder );
				dataSubFolder = value; }
		}
        
        public ModelInterface TopModel {
        	get { if( loaderInstance != null) {
        			return loaderInstance.TopModel;
        		} else {
        			return null;
        		}
        	}
        }
        
		public ushort ServicePort {
			get { return servicePort; }
			set { servicePort = value; }
		}

        public int ReplaySpeed
        {
            get { return replaySpeed; }
            set { replaySpeed = value; }
        }

        public int BreakAtBar
        {
            get { return breakAtBar; }
            set { breakAtBar = value; }
        }

        private ChartType chartType = ChartType.Bar;
        public Func<Chart> createChart;
        private BarUnit defaultBarUnit = BarUnit.Hour;
        private int defaultPeriod = 1;
        private bool disableCharting;

        private bool enableAlarmSounds;
        private DateTime endDateTime;
        private BarUnit engineBarUnit = BarUnit.Hour;
        private int enginePeriod = 1;
        private bool failedAlarmSound;
        private Action flushCharts = () => { };
        private Interval initialInterval;
        private Interval intervalChartBar;
        private Interval intervalDefault;
        private Interval intervalEngine;
        private bool isEngineLoaded;
        private DateTime maxDateTime;
        private DateTime minDateTime;
        private string modelLoader;

        // The progress of the task in percentage
        private int percentProgress;
        private string progressText;

        private int replaySpeed;
        private Action showChart;
        private DateTime startDateTime;
        private string symbolList;
        private Exception taskException;
        private bool useDefaultInterval = true;
        private Dictionary<string, string> starters = new Dictionary<string, string>()
        {
            { "Historical Simulation", "HistoricalStarter"},
            { "Optimization a.k.a. Brute Force", "OptimizeStarter"},
            { "Genetic Optimization", "GeneticStarter"},
            { "Realtime Simulation", "TestRealTimeStarter"},
            { "FIX Server Simulation", "FIXSimulatorStarter"},
            { "Realtime Operation (Demo or Live)", "RealTimeStarter"},
        };

        private string starter;

        public List<string> StarterValues
        {
            get {
                var labels = new List<string>();
                foreach( var kvp in starters)
                {
                    labels.Add(kvp.Key);
                }
                return labels;
            }
        }

        public string Starter
        {
            get { return starter; }
            set { NotifyOfPropertyChange(() => Starter);
            	starter = value;
            }
        }

        public bool DisableCharting
        {
            get { return disableCharting; }
            set
            {
                NotifyOfPropertyChange(() => DisableCharting);
                disableCharting = value;
            }
        }

        public string ModelLoader
        {
            get { return modelLoader; }
            set
            {
                NotifyOfPropertyChange(() => ModelLoader);
                modelLoader = value;
            }
        }

        public string ProgressText
        {
            get { return progressText; }
            set
            {
                NotifyOfPropertyChange(() => ProgressText);
                progressText = value;
            }
        }

        #endregion Fields

        #region Constructors

        public StarterConfig()
            : this("project")
        {
        }

        public StarterConfig(string projectName)
        {
            ConfigurationManager.AppSettings.Set("ProviderAddress", "InProcess");
            log = Factory.SysLog.GetLogger(typeof (StarterConfig));
            progressChildren = new Dictionary<int, Progress>();
            string storageFolder = Factory.Settings["AppDataFolder"];
            string workspace = Path.Combine(storageFolder, "Workspace");
            string projectFile = Path.Combine(workspace, projectName + ".tzproj");
            projectConfig = new ConfigFile(projectFile, GetDefaultConfig());
            commandWorker = new BackgroundWorker();
            commandWorker.WorkerReportsProgress = true;
            commandWorker.WorkerSupportsCancellation = true;
            commandWorker.DoWork += ProcessWorkerDoWork;
            commandWorker.RunWorkerCompleted += ProcessWorkerRunWorkerCompleted;
            commandWorker.ProgressChanged += ProcessWorkerProgressChanged;
            intervalDefault = initialInterval;
            intervalEngine = initialInterval;
            intervalChartBar = initialInterval;
            Array units = Enum.GetValues(typeof (BarUnit));
            minDateTime = new DateTime(1800, 1, 1);
            startDateTime = minDateTime;
            maxDateTime = DateTime.Now.AddDays(1).AddSeconds(-1);
            endDateTime = maxDateTime;
            Load();
            TryAutoUpdate();
        }

        #endregion Constructors

        #region Properties

        public string AlarmFile
        {
            get { return alarmFile; }
            set { alarmFile = value; }
        }

        public bool BarChartEnabled
        {
            get { return chartType == ChartType.Bar; }
        }

        public bool CanStart
        {
            get { return IsEngineLoaded && !commandWorker.IsBusy; }
            set { NotifyOfPropertyChange(() => CanStart); }
        }

        public bool CanTryAutoUpdate
        {
            get { return !commandWorker.IsBusy; }
            set { NotifyOfPropertyChange(() => CanTryAutoUpdate); }
        }

        public BarUnit ChartBarUnit
        {
            get { return chartBarUnit; }
            set
            {
                NotifyOfPropertyChange(() => ChartBarUnit);
                chartBarUnit = value;
            }
        }

        public int ChartPeriod
        {
            get { return chartPeriod; }
            set
            {
                NotifyOfPropertyChange(() => ChartPeriod);
                chartPeriod = value;
            }
        }

        public ChartType ChartType
        {
            get { return chartType; }
            set
            {
                NotifyOfPropertyChange(() => ChartType);
                chartType = value;
            }
        }

        public BackgroundWorker CommandWorker
        {
            get { return commandWorker; }
        }

        public Func<Chart> CreateChart
        {
            get { return createChart; }
            set
            {
                NotifyOfPropertyChange(() => CreateChart);
                createChart = value;
            }
        }

        public BarUnit DefaultBarUnit
        {
            get { return defaultBarUnit; }
            set
            {
                NotifyOfPropertyChange(() => DefaultBarUnit);
                defaultBarUnit = value;
            }
        }

        public int DefaultPeriod
        {
            get { return defaultPeriod; }
            set
            {
                NotifyOfPropertyChange(() => DefaultPeriod);
                defaultPeriod = value;
            }
        }

        public bool EnableAlarmSounds
        {
            get { return enableAlarmSounds; }
        }

        public bool EnableChartBarsChoice
        {
            get { return !useDefaultInterval; }
        }

        public bool EnableEngineBarsChoice
        {
            get { return !useDefaultInterval; }
        }

        public DateTime EndDateTime
        {
            get { return endDateTime; }
            set
            {
                NotifyOfPropertyChange(() => EndDateTime);
                endDateTime = value;
            }
        }

        public BarUnit EngineBarUnit
        {
            get { return engineBarUnit; }
            set
            {
                NotifyOfPropertyChange(() => EngineBarUnit);
                engineBarUnit = value;
            }
        }

        public int EnginePeriod
        {
            get { return enginePeriod; }
            set
            {
                NotifyOfPropertyChange(() => EnginePeriod);
                enginePeriod = value;
            }
        }

        public bool EnginePeriodEnabled
        {
            get { return !useDefaultInterval; }
            set { NotifyOfPropertyChange(() => EnginePeriodEnabled); }
        }

        public Action FlushCharts
        {
            get { return flushCharts; }
            set
            {
                NotifyOfPropertyChange(() => FlushCharts);
                flushCharts = value;
            }
        }

        public bool IsEngineLoaded
        {
            get
            {
                NotifyOfPropertyChange(() => IsEngineLoaded);
                return isEngineLoaded;
            }
        }

        public DateTime MaxDateTime
        {
            get { return maxDateTime; }
            set
            {
                NotifyOfPropertyChange(() => MaxDateTime);
                maxDateTime = value;
            }
        }

        public DateTime MinDateTime
        {
            get { return minDateTime; }
            set
            {
                NotifyOfPropertyChange(() => MinDateTime);
                minDateTime = value;
            }
        }

        public List<string> ModelLoaderValues
        {
            get
            {
                Plugins plugins = Plugins.Instance;
                List<ModelLoaderInterface> loaders = plugins.GetLoaders();
                var modelLoaderList = new List<string>();
                for (int i = 0; i < loaders.Count; i++)
                {
                    if (loaders[i].IsVisibleInGUI)
                    {
                        modelLoaderList.Add(loaders[i].Name);
                    }
                }
                return modelLoaderList;
            }
        }

        public int PercentProgress
        {
            get { return percentProgress; }
            set
            {
                NotifyOfPropertyChange(() => PercentProgress);
                percentProgress = value;
            }
        }

        public Action ShowChart
        {
            get { return showChart; }
            set
            {
                NotifyOfPropertyChange(() => ShowChart);
                showChart = value;
            }
        }

        public DateTime StartDateTime
        {
            get { return startDateTime; }
            set
            {
                NotifyOfPropertyChange(() => StartDateTime);
                startDateTime = value;
            }
        }

        public string SymbolList
        {
            get { return symbolList; }
            set
            {
                NotifyOfPropertyChange(() => SymbolList);
                symbolList = value;
            }
        }

        public Exception TaskException
        {
            get { return taskException; }
            set
            {
                NotifyOfPropertyChange(() => TaskException);
                taskException = value;
            }
        }

        public bool UseOtherIntervals
        {
            get { return !useDefaultInterval; }
        }

        public bool UseDefaultInterval
        {
            get { return useDefaultInterval; }
            set
            {
                NotifyOfPropertyChange(() => UseDefaultInterval);
                NotifyOfPropertyChange(() => UseOtherIntervals);
                useDefaultInterval = value;
            }
        }

        #endregion Properties

        #region Methods
        
        public bool IsBusy {
            get { return commandWorker.IsBusy; }
            set { NotifyOfPropertyChange(() => IsBusy); }
        }

        public bool CanStop
        {
            get { return commandWorker.IsBusy; }
            set { NotifyOfPropertyChange(() => CanStop); }
        }

        public void Dispose()
        {
            Stop();
            Factory.Engine.Dispose();
            Factory.Provider.Release();
            Factory.TickUtil.TickReader().CloseAll();
        }

        public static string GetDefaultConfig()
        {
            return
                @"<?xml version=""1.0"" encoding=""utf-8""?>
            <configuration>
            <appSettings>
            <clear />
            <add key=""StartTime"" value=""Wednesday, January 01, 1800"" />
            <add key=""EndTime"" value=""Thursday, July 23, 2009"" />
            <add key=""AutoUpdate"" value=""true"" />
            <add key=""Symbol"" value=""GBP/USD,EUR/JPY"" />
            <add key=""UseModelLoader"" value=""true"" />
            <add key=""AlarmSound"" value=""..\..\Media\59642_AlternatingToneAlarm.wav"" />
            <add key=""ModelLoader"" value=""Example: Reversal Multi-Symbol"" />
            <add key=""Model"" value=""ExampleReversalStrategy"" />
            <add key=""MaxParallelPasses"" value=""1000"" />
            <add key=""DefaultPeriod"" value=""1"" />
            <add key=""Starter"" value=""HistoricalStarter"" />
            <add key=""DefaultInterval"" value=""Hour"" />
            <add key=""EnginePeriod"" value=""1"" />
            <add key=""EngineInterval"" value=""Hour"" />
            <add key=""ChartPeriod"" value=""1"" />
            <add key=""ChartInterval"" value=""Hour"" />
            <add key=""ServiceAddress"" value=""InProcess"" />
            <add key=""ServicePort"" value=""6490"" />
            <add key=""ProviderAssembly"" value=""MBTFIXProvider"" />
            </appSettings>
            </configuration>";
        }

        public void Catch()
        {
            if (taskException != null)
            {
                throw new ApplicationException(taskException.Message, taskException);
            }
        }

        public void CheckForEngine()
        {
            try
            {
                TickEngine engine = Factory.Engine.TickEngine;
                isEngineLoaded = true;
            }
            catch (Exception)
            {
                string msg = "Sorry, cannot find an engine compatible with this version.";
                log.Notice(msg);
            }
            if (isEngineLoaded)
            {
                initialInterval = Factory.Engine.DefineInterval(BarUnit.Day, 1);
            }
        }

        public void CopyDefaultIntervals()
        {
            enginePeriod = defaultPeriod;
            engineBarUnit = defaultBarUnit;
            chartPeriod = defaultPeriod;
            chartBarUnit = defaultBarUnit;
        }

        public void Start()
        {
        	string starterName;
        	if( !starters.TryGetValue(starter, out starterName)) {
        		starterName = starter;
        	}
            var starterInstance = Factory.Starter.CreateStarter( starterName);
            if (starterName.Contains("RealTime"))
            {
                enableAlarmSounds = true;
            }
            SetupStarter(starterInstance);
        }

        public void IntervalsUpdate()
        {
            intervalDefault = Factory.Engine.DefineInterval(defaultBarUnit, defaultPeriod);
            intervalEngine = Factory.Engine.DefineInterval(engineBarUnit, enginePeriod);
            intervalChartBar = Factory.Engine.DefineInterval(chartBarUnit, chartPeriod);
        }


        public void RunCommand(CommandInterface command)
        {
            Save();
            CommandWorker.RunWorkerAsync(command);
        }

        public void Stop()
        {
            commandWorker.CancelAsync();
            PercentProgress = 0;
            ProgressText = "Execution Stopped";
        }

        public void TryAutoUpdate()
        {
            string autoUpdateFlag = projectConfig.GetValue("AutoUpdate");
            if ("true".Equals(autoUpdateFlag))
            {
                commandWorker.RunWorkerAsync(new ActionCommand(DoAutoUpdate));
            }
            else
            {
                log.Notice("To enable AutoUpdate, set AutoUpdate 'true' in " + projectConfig);
                CheckForEngine();
            }
        }

        private void AlarmTimerTick(object sender, EventArgs e)
        {
            PlayAlarmSound();
        }

        private string CheckNull(string str)
        {
            if (str == null)
            {
                return "";
            }
            else
            {
                return str;
            }
        }

        private void DoAutoUpdate()
        {
            if (Factory.AutoUpdate(commandWorker))
            {
                log.Notice("AutoUpdate succesful. Restart unnecessary.");
            }
            CheckForEngine();
        }

        private void Load()
        {
            servicePort = (ushort) projectConfig.GetValue("ServicePort", typeof (ushort));
            dataSubFolder = projectConfig.GetValue("DataSubFolder");
            alarmFile = projectConfig.GetValue("AlarmSound");
            if (string.IsNullOrEmpty(alarmFile))
            {
                alarmFile = @"..\..\Media\59642_AlternatingToneAlarm.wav";
            }
            var disableChartingString = projectConfig.GetValue("DisableCharting");
            disableCharting = !string.IsNullOrEmpty(disableChartingString) &&
                              "true".Equals(disableChartingString.ToLower());

            var startTimeStr = projectConfig.GetValue("StartTime");
            var endTimeStr = projectConfig.GetValue("EndTime");
            modelLoader = projectConfig.GetValue("ModelLoader");

            symbolList = projectConfig.GetValue("Symbol");

            if (startTimeStr != null)
            {
                try
                {
                    startDateTime = new TimeStamp(startTimeStr).DateTime;
                }
                catch
                {
                    startDateTime = minDateTime;
                }
            }

            if (endTimeStr != null)
            {
                try
                {
                    var time = new TimeStamp(endTimeStr).DateTime.AddDays(1).AddSeconds(-1);
                    EndDateTime = time;
                }
                catch
                {
                    endDateTime = maxDateTime;
                }
                if (endDateTime > maxDateTime)
                {
                    endDateTime = maxDateTime;
                }
            }
            defaultPeriod = int.Parse(CheckNull(projectConfig.GetValue("DefaultPeriod")));
            defaultBarUnit =
                (BarUnit) Enum.Parse(typeof (BarUnit), CheckNull(projectConfig.GetValue("DefaultInterval")));
            enginePeriod = int.Parse(CheckNull(projectConfig.GetValue("EnginePeriod")));
            engineBarUnit = (BarUnit) Enum.Parse(typeof (BarUnit), CheckNull(projectConfig.GetValue("EngineInterval")));
            chartPeriod = int.Parse(CheckNull(projectConfig.GetValue("ChartPeriod")));
            chartBarUnit = (BarUnit) Enum.Parse(typeof (BarUnit), CheckNull(projectConfig.GetValue("ChartInterval")));
            var starterFactoryName = projectConfig.GetValue("Starter");
            starter = GetStarterByFactoryName( starterFactoryName);
        }
        
        private string GetStarterByFactoryName(string name) {
        	foreach( var kvp in starters) {
        		if( kvp.Value == name) {
        			return kvp.Key;
        		}
        	}
        	return null;
        }

        public void Save()
        {
            projectConfig.SetValue("ServicePort", servicePort.ToString());
            projectConfig.SetValue("DisableCharting", disableCharting ? "true" : "false");
            projectConfig.SetValue("StartTime", new TimeStamp(startDateTime).ToString());
            projectConfig.SetValue("EndTime", new TimeStamp(endDateTime).ToString());
            projectConfig.SetValue("Symbol", symbolList);
            projectConfig.SetValue("ModelLoader", ModelLoader);
            projectConfig.SetValue("AutoUpdate", projectConfig.GetValue("AutoUpdate"));

            // Intervals
            projectConfig.SetValue("DefaultPeriod", defaultPeriod.ToString());
            projectConfig.SetValue("DefaultInterval", defaultBarUnit.ToString());
            projectConfig.SetValue("EnginePeriod", enginePeriod.ToString());
            projectConfig.SetValue("EngineInterval", engineBarUnit.ToString());
            projectConfig.SetValue("ChartPeriod", chartPeriod.ToString());
            projectConfig.SetValue("ChartInterval", chartBarUnit.ToString());
            string starterFactoryName;
            if( starter == null || !starters.TryGetValue(starter, out starterFactoryName)) {
               	starterFactoryName = starter;
            }
            if( string.IsNullOrEmpty(starterFactoryName)) {
            	starterFactoryName = "";
            }
            projectConfig.SetValue("Starter", starterFactoryName);
            projectConfig.SetValue("AutoUpdate", "false");
            projectConfig.SetValue("AutoUpdate", projectConfig.GetValue("AutoUpdate"));
            projectConfig.SetValue("DataSubFolder", dataSubFolder);
        }

        private void PlayAlarmSound()
        {
            if (!failedAlarmSound)
            {
                string alarmFile = projectConfig.GetValue("AlarmSound");
                if (string.IsNullOrEmpty(alarmFile))
                {
                    alarmFile = @"..\..\Media\59642_AlternatingToneAlarm.wav";
                }
                try
                {
                    var simpleSound = new SoundPlayer(alarmFile);
                    simpleSound.Play();
                }
                catch (Exception ex)
                {
                    failedAlarmSound = true;
                    log.Error("Failure playing alarm sound file " + alarmFile + " : " + ex.Message, ex);
                }
            }
        }

        private void ProcessWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            UpdateStatus();
            var bw = sender as BackgroundWorker;
            var command = (CommandInterface) e.Argument;
            if (log.IsTraceEnabled) log.Trace("Started background worker.");
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "CommandWorker";
            }
            if (command.CanExecute)
            {
                command.Execute();
            }
        }

        private void UpdateStatus()
        {
        	// value ignore. Just forces update.
            CanStart = true;
            CanStop = true;
            CanTryAutoUpdate = true;
            IsBusy = true;
        }

        private void ProcessWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var progress = (Progress) e.UserState;
            progressChildren[progress.Id] = progress;

            long final = 0;
            long current = 0;
            foreach (var kvp in progressChildren)
            {
                Progress child = kvp.Value;
                final += child.Final;
                current += child.Current;
            }

            // Calculate the task progress in percentages
            if (final > 0)
            {
                PercentProgress = Convert.ToInt32((current*100)/final);
            }
            else
            {
                PercentProgress = 0;
            }
            ProgressText = progress.Text + ": " + progress.Current + " out of " + progress.Final + " (" +
                           PercentProgress + "%)";
        }

        private void ProcessWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            enableAlarmSounds = false;
            taskException = e.Error;
            if (taskException != null)
            {
                if (taskException.InnerException is TickZoomException)
                {
                    log.Error(taskException.InnerException.Message, taskException.InnerException);
                }
                else
                {
                    log.Error(taskException.Message, taskException);
                }
            }
            UpdateStatus();
        }

        private void SetupStarter(Starter starterInstance)
        {
            FlushCharts();
            IntervalsUpdate();
            starterInstance.ProjectProperties.Starter.StartTime = (TimeStamp) startDateTime;
            starterInstance.ProjectProperties.Starter.EndTime = (TimeStamp)endDateTime;
            starterInstance.BackgroundWorker = commandWorker;
            starterInstance.DataFolder = dataSubFolder;
            if (!disableCharting)
            {
                if (CreateChart == null || ShowChart == null)
                {
                    log.Warn("Charting is enabled but you never set one of the CreateChart or the ShowChart properties.");
                }
                else
                {
                    starterInstance.CreateChartCallback = new CreateChartCallback(CreateChart);
                    starterInstance.ShowChartCallback = new ShowChartCallback(ShowChart);
                }
            }
            else
            {
                log.Notice("You have the \"disable charts\" check box enabled.");
            }
            starterInstance.ProjectProperties.Chart.ChartType = chartType;
            starterInstance.ProjectProperties.Starter.SetSymbols(symbolList);
            starterInstance.ProjectProperties.Starter.IntervalDefault = intervalDefault;
//            starterInstance.Address = projectConfig.GetValue("ServiceAddress");
//            starterInstance.Config = projectConfig.GetValue("ServiceConfig");
            starterInstance.Port = servicePort;
//            starterInstance.AddProvider(projectConfig.GetValue("ProviderAssembly"));
            if (useDefaultInterval)
            {
                starterInstance.ProjectProperties.Chart.IntervalChartBar = intervalDefault;
            }
            else 
            {
                starterInstance.ProjectProperties.Chart.IntervalChartBar = intervalChartBar;
            }
            if (intervalChartBar.BarUnit == BarUnit.Default)
            {
                starterInstance.ProjectProperties.Chart.IntervalChartBar = intervalDefault;
            }
            log.Info("Running Loader named: " + modelLoader);
            loaderInstance = Plugins.Instance.GetLoader(modelLoader);
            RunCommand(new StarterCommand(starterInstance, loaderInstance));
        }

        #endregion Methods
    }
}