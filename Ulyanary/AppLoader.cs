using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Ulyanary
{
    internal class AppLoader
    {
        private static volatile object _locker = new object();
        private static AppLoader _instance;
        private bool _stopping;
        private static ConfigLoader _configLoader;
        public static ConfigData LoadedConfig => _configLoader.LoadedConfig;
        private static MqttSubscriper _mqttSubscriper;

        public static AppLoader Instance
        {
            get
            {
                lock (_locker)
                {
                    _instance ??= new AppLoader();
                }
                return _instance;
            }
        }
        public static void LoadConfig()
        {
            _configLoader ??= new ConfigLoader();
            _configLoader.LoadConfig();
        }
        public static void StartMqttSubscription()
        {
            _mqttSubscriper ??= new MqttSubscriper(LoadedConfig, new FalconConsumer(LoadedConfig));
        }


        private AppLoader()
        {
            new Thread(RunLoop).Start();
        }
        private void RunLoop()
        {
            while (!_stopping)
            {
                Thread.Sleep(10000);
            }
        }

        public void Stop()
        {
            _stopping = true;
        }
    }
}
