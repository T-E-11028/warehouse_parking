using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;
using O2DESNet.Distributions;
namespace warehouse_parking
{
    internal class warehouseParkingModel : Sandbox
    {
        // 静态变量
        /// <summary>
        /// 车辆到达率
        /// </summary>
        public double ArrivalRate { get; set; }

        /// <summary>
        /// 路径处理时间下限
        /// </summary>
        public double PathLow { get; set; }

        /// <summary>
        /// 路径处理时间上限
        /// </summary>
        public double PathUp { get; set; }

        /// <summary>
        /// 停车场容量
        /// </summary>
        public int ParkingCapacity { get; set; }

        /// <summary>
        /// 路径容量
        /// </summary>
        public int PathCapacity { get; set; }

        /// <summary>
        /// 仓库容量
        /// </summary>
        public int WarehouseCapacity { get; set; }

        /// <summary>
        /// 路径服务率
        /// </summary>
        public double PathServiceRate { get; set; }

        /// <summary>
        /// 仓库服务率
        /// </summary>
        public double WarehouseServiceRate { get; set; }

        // 动态变量


        /// <summary>
        /// 仿真时间
        /// </summary>
        public double SimulationClock {  get; set; }

        /// <summary>
        /// 车辆进入停车场的时间
        /// </summary>
        public List<double> ArrivalSeconds { get; set; }

        /// <summary>
        /// 车辆等待时间
        /// </summary>
        public List<double> WaitingSeconds { get; set; }

        /// <summary>
        /// 停车场中的顾客数量（区分是否空闲）
        /// </summary>
        public int NumberInParking { get; set; }

        /// <summary>
        /// 路径中的顾客数量（区分是否空闲）
        /// </summary>
        public int NumberInPath { get; set; }

        /// <summary>
        /// 仓库中的顾客数量（区分是否空闲）
        /// </summary>
        public int NumberInWarehouse { get; set; }

        /// <summary>
        /// 车辆等待时间
        /// </summary>
        public int CarWaitingTime { get; set; }

        /// <summary>
        /// 已到达的车辆总数，用于计算平均服务时长
        /// </summary>
        public int totalCarArrived { get; set; }



        // 事件
        /// <summary>
        /// 生成车辆
        /// </summary>
        void Generate()
        {
            // 触发下一个事件：判断是否进入停车场，时钟更新：无
            if (NumberInParking < ParkingCapacity)
            {
                // 停车场未满，进入，否则离开
                Schedule(() => EnPark(), TimeSpan.Zero);
                
            }
            // 服务时长-分钟
            double serviceMunites = O2DESNet.Distributions.Exponential.Sample(DefaultRS, ArrivalRate);
            // 更新仿真时钟
            SimulationClock += serviceMunites * 60;
            // 触发下一个生成器（等于循环），间隔时间是上面指定的到达率的指数分布
            Schedule(() => Generate(), TimeSpan.FromMinutes(serviceMunites));
        }

        /// <summary>
        /// 顾客进入停车场
        /// </summary>
        void EnPark()
        {
            // 停车场车辆数量+1
            NumberInParking++;
            // 记录车辆到达时间
            ArrivalSeconds.Add(SimulationClock);
            // 触发下一个事件：尝试进入路径
            Schedule(() => AttempEnPath(), TimeSpan.Zero);
        }

        /// <summary>
        /// 尝试进入路径
        /// </summary>
        void AttempEnPath()
        {
            // 判断服务台是否空闲
            if (NumberInWarehouse < WarehouseCapacity)
            {
                // 空闲，则进入路径
                Schedule(() => EnPath(), TimeSpan.Zero);
            }
        }

        /// <summary>
        /// 进入路径
        /// </summary>
        void EnPath()
        {
            // 路径人数加1
            NumberInPath++;
            // 停车场人数-1
            NumberInParking--;
            // 计算车辆等待时间
            double ArrivalTime = ArrivalSeconds[0];
            double WaitingTime = SimulationClock - ArrivalTime;
            // 添加到list中存储
            WaitingSeconds.Add(WaitingTime);

            //从队列中移除第一辆车
            ArrivalSeconds.RemoveAt(0);

            // 更新仿真时钟
            double ServiceTime = Uniform.Sample(DefaultRS, PathLow, PathUp);
            SimulationClock += ServiceTime * 60;
            // 触发下一个事件：进入仓库
            Schedule(() => EnWarehouse(), TimeSpan.FromMinutes(ServiceTime));
        }

        /// <summary>
        /// 进入仓库
        /// </summary>
        void EnWarehouse()
        {
            // 路径人数-1
            NumberInPath--;
            // 仓库人数+1
            NumberInWarehouse++;

            // 更新仿真时钟
            double ServiceTime = O2DESNet.Distributions.Exponential.Sample(DefaultRS, WarehouseServiceRate);
            SimulationClock += ServiceTime * 60;
            // 触发下一个事件：离开仓库
            Schedule(() => DeWareHouse(), TimeSpan.FromMinutes(ServiceTime));

        }

        /// <summary>
        /// 离开仓库
        /// </summary>
        void DeWareHouse()
        {
            // 仓库人数-1
            NumberInWarehouse--;
            // 若停车场有车等待，则触发进入事件
            if (NumberInParking > 0)
            {
                Schedule(() => EnPath(), TimeSpan.Zero);
            }
        }


        /// <summary>
        /// 构造函数，实例化时自动执行，实际相当于主函数
        /// </summary>
        /// <param name="seed">随机数种子</param>
        public warehouseParkingModel(int seed = 1):base(seed)
        {
            // 初始化
            // 静态变量
            ArrivalRate = 6;
            PathLow = 3;
            PathUp = 5;
            WarehouseServiceRate = 2;
            ParkingCapacity = 10;
            PathCapacity = 1;
            WarehouseCapacity = 1;
            WaitingSeconds = new List<double>();
            ArrivalSeconds = new List<double>();

            // 动态变量无需初始化，不定义默认为0
            // 更新仿真时钟
            double ServiceTime = O2DESNet.Distributions.Exponential.Sample(DefaultRS, ArrivalRate);
            SimulationClock += ServiceTime * 60;
            // 开始仿真
            Schedule(() => Generate(), TimeSpan.FromMinutes(ServiceTime));
        }
        }
    }
