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
        /// 车辆到达时间间隔
        /// </summary>
        public double ArrivalInterval { get; set; }

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
        /// 仓库服务时长
        /// </summary>
        public double WarehouseServiceInterval { get; set; }

        // 动态变量

        /// <summary>
        /// 所有已到达的车
        /// </summary>
        public List<Car> CarsArrived { get; set; } = new List<Car>();

        /// <summary>
        /// 停车场中的车
        /// </summary>
        public List<Car> CarsInParking { get; set; } = new List<Car>();

        /// <summary>
        /// 路径中的车
        /// </summary>
        public List<Car> CarsInPath {  get; set; } = new List<Car>();

        /// <summary>
        /// 仓库中的车
        /// </summary>
        public List<Car> CarsInWarehouse { get; set; } = new List<Car>();

        /// <summary>
        /// 停车场中的顾客数量（区分是否空闲）
        /// </summary>
        public int NumberInParking { get { return CarsInParking.Count; } }

        /// <summary>
        /// 路径中的顾客数量（区分是否空闲）
        /// </summary>
        public int NumberInPath { get { return CarsInPath.Count; } }

        /// <summary>
        /// 仓库中的顾客数量（区分是否空闲）
        /// </summary>
        public int NumberInWarehouse { get { return CarsInWarehouse.Count; } }



        // 事件

        /// <summary>
        /// 生成车辆
        /// </summary>
        void Generate(Car car)
        {
            // 触发下一个事件：判断是否进入停车场，时钟更新：无
            if (NumberInParking < ParkingCapacity)
            {
                // 停车场未满，进入，否则离开
                Schedule(() => EnPark(car), TimeSpan.Zero);
                
            }

            // 触发下一个生成器（等于循环），间隔时间是上面指定的到达率的指数分布
            Schedule(() => Generate(new Car()), TimeSpan.FromMinutes(Exponential.Sample(DefaultRS, ArrivalInterval)));

        }

        /// <summary>
        /// 顾客进入停车场
        /// </summary>
        void EnPark(Car car)
        {
            // 停车场车辆数量+1
            CarsInParking.Add(car);
            // 已到达车辆数量+1
            CarsArrived.Add(car);
            // 记录车辆到达时间
            car.arrivalDateTime = ClockTime;
            // 触发下一个事件：尝试进入路径
            Schedule(() => AttempEnPath(car), TimeSpan.Zero);

            Console.WriteLine($"车{car.carId}进入停车场，停车场数量：{NumberInParking}，路径数量：{NumberInPath}，仓库数量：{NumberInWarehouse}");
        }

        /// <summary>
        /// 尝试进入路径
        /// </summary>
        void AttempEnPath(Car car)
        {
            // 判断服务台是否空闲
            if (NumberInWarehouse < WarehouseCapacity && NumberInPath < PathCapacity)
            {
                // 空闲，则进入路径
                Schedule(() => EnPath(car), TimeSpan.Zero);
            }
        }

        /// <summary>
        /// 进入路径
        /// </summary>
        void EnPath(Car car)
        {
            // 停车场人数-1
            CarsInParking.Remove(car);

            // 路径人数加1
            CarsInPath.Add(car);

            // 计算车辆等待时间
            car.enterServerDateTime = ClockTime;
            car.calWaitingTime();

            
            // 触发下一个事件：进入仓库
            Schedule(() => EnWarehouse(car), TimeSpan.FromMinutes(Uniform.Sample(DefaultRS, PathLow, PathUp)));

            Console.WriteLine($"车{car.carId}进入路径，停车场数量：{NumberInParking}，路径数量：{NumberInPath}，仓库数量：{NumberInWarehouse}");

        }

        /// <summary>
        /// 进入仓库
        /// </summary>
        void EnWarehouse(Car car)
        {
            // 路径人数-1
            CarsInPath.Remove(car);
            // 仓库人数+1
            CarsInWarehouse.Add(car);

            // 触发下一个事件：离开仓库
            Schedule(() => DeWareHouse(car), TimeSpan.FromMinutes(Exponential.Sample(DefaultRS, WarehouseServiceInterval)));

            Console.WriteLine($"车{car.carId}进入仓库，停车场数量：{NumberInParking}，路径数量：{NumberInPath}，仓库数量：{NumberInWarehouse}");

        }

        /// <summary>
        /// 离开仓库
        /// </summary>
        void DeWareHouse(Car car)
        {
            // 仓库人数-1
            CarsInWarehouse.Remove(car);
            // 若停车场有车等待，则触发进入事件
            if (NumberInParking > 0)
            {
                Schedule(() => EnPath(CarsInParking.First()), TimeSpan.Zero);
            }
            Console.WriteLine($"车{car.carId}离开仓库，停车场数量：{NumberInParking}，路径数量：{NumberInPath}，仓库数量：{NumberInWarehouse}");

        }


        /// <summary>
        /// 构造函数，实例化时自动执行，实际相当于主函数
        /// </summary>
        /// <param name="seed">随机数种子</param>
        public warehouseParkingModel(int seed = 1):base(seed)
        {
            // 初始化
            // 静态变量
            ArrivalInterval = 6;
            PathLow = 3;
            PathUp = 5;
            WarehouseServiceInterval = 2;
            ParkingCapacity = 10;
            PathCapacity = 1;
            WarehouseCapacity = 1;

            // 开始仿真
            Schedule(() => Generate(new Car()), TimeSpan.FromMinutes(Exponential.Sample(DefaultRS, ArrivalInterval)));
        }
        }
    }
