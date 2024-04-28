using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace warehouse_parking
{   
    public class Car
    {
        internal static int id {  get; set; } = 0;

        public int carId { get; set; }

        // 车辆到达时间
        public DateTime arrivalDateTime { get; set; }

        // 车辆进入服务器时间（离开队列时间）
        public DateTime enterServerDateTime { get; set; }

        // 车辆等待时间
        public TimeSpan waitingTime { get; set; }

        // 车辆等待时间（秒）
        public double waitingTimeSecond { get { return waitingTime.TotalSeconds; } }

        // 车辆等待时间（分钟）
        public double waitingTimeMinutes { get { return waitingTime.TotalMinutes; } }

        public Car()
        {
            carId = id;
            id++;
        }

        /// <summary>
        /// 计算等待时长
        /// </summary>
        public void calWaitingTime()
        {
            if (enterServerDateTime != null && arrivalDateTime != null)
            {
                waitingTime = enterServerDateTime - arrivalDateTime;
            }
            /**else
            {
                if (enterServerDateTime == null)
                {
                    throw Exception("In calWaitingTime: enterServerDateTime is null");
                }
                else if (arrivalDateTime == null)
                {
                    throw Exception("In calWaitingTime: arrivalDateTime is null");
                }
            }**/
            
        }
    }
}
