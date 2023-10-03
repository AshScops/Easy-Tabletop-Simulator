using StackExchange.Redis;
namespace Tabletop
{
    class RedisHelper
    {
        //120.76.196.221:6379
        private static readonly ConfigurationOptions ConfigurationOptions =
            ConfigurationOptions.Parse("localhost:6379,password=8Mk_2pPowhjbiwj82nbMa_jwbuBfKiszq");
        private static readonly object Locker = new object();
        private static ConnectionMultiplexer _redisConn;

        /// <summary>
        /// ������ȡ
        /// </summary>
        public static ConnectionMultiplexer RedisConn
        {
            get
            {
                if (_redisConn == null)
                {
                    // ����ĳһ����飬��ͬһʱ��ֻ��һ���̷߳��ʸô����
                    lock (Locker)
                    {
                        if (_redisConn == null || !_redisConn.IsConnected)
                        {
                            _redisConn = ConnectionMultiplexer.Connect(ConfigurationOptions);
                        }
                    }
                }
                return _redisConn;
            }
        }

    }
}