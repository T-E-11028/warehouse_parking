using MathNet.Numerics.Statistics;

namespace warehouse_parking
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Random r = new Random();
            warehouseParkingModel warehouseParkingModel1 = new warehouseParkingModel(r.Next());
            warehouseParkingModel1.Run(TimeSpan.FromMinutes(1440));

            List<double> waitingTimeMinutes = new List<double>();

            // 将在停车场中等待的车辆赋值最终时间，以计算等待时间
            foreach (Car car in warehouseParkingModel1.CarsArrived)
            {
                // 若不存在，则赋值最终时间
                if (Object.Equals(car.enterServerDateTime, default(DateTime)))
                {
                    car.enterServerDateTime = warehouseParkingModel1.ClockTime;
                    car.calWaitingTime();
                }
            }

            // 获取所有车辆等待时长
            foreach (Car car in warehouseParkingModel1.CarsArrived) {
                waitingTimeMinutes.Add(car.waitingTimeMinutes);
            }

            Console.WriteLine($"average waiting minutes: {waitingTimeMinutes.Mean()}");
        }
    }
}