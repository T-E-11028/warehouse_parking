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
            Console.WriteLine($"average waiting minutes: {warehouseParkingModel1.WaitingSeconds.Mean()/60}");
        }
    }
}