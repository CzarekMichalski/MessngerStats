using System;
using System.Collections.Generic;
using System.Linq;
using DataLoader.DB.Entities;

namespace DataLoader.Utils
{
    public class MessagesToHourActivity
    {
        public static Dictionary<DateTime, int> Convert(List<MessageEntity> messages)
        {
            var period = 15;
            var hourActivityDictionary = new Dictionary<DateTime, int>();
            var hourList = new List<DateTime>();

            messages.ForEach(x => hourList.Add(new DateTime(2020, 1, 1, x.SendTime.Hour, x.SendTime.Minute, x.SendTime.Second)));
            hourList = hourList.OrderBy(x => x).ToList();

            var sum = 0;
            var time = new DateTime(2020, 1, 1, 0, 0, 0);

            foreach (var hour in hourList)
            {
                if (hour <= time.AddMinutes(period))
                {
                    sum++;
                }
                else
                {
                    hourActivityDictionary.Add(time, sum);
                    sum = 0;
                    time = time.AddMinutes(period);
                }
            }

            return hourActivityDictionary;
        }
    }
}