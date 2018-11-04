using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

namespace HyUtilities
{
    /// <summary>
    /// 计时的计时单位
    /// </summary>
    public enum TimeUnit : int
    {
        /// <summary>
        /// 按秒计时
        /// </summary>
        Second = 0,
        /// <summary>
        /// 按分计时
        /// </summary>
        Minute = 1,
        /// <summary>
        /// 按小时计时
        /// </summary>
        Hour = 2,
    }

    public class HyTick
    {
        [DllImport("kernel32.dll")]
        public static extern int GetTickCount();

        /// <summary>
        /// 取得毫秒数
        /// </summary>
        /// <returns></returns>
        public static int TickTimeGet()
        {
            return GetTickCount();
        }

        /// <summary>
        /// 延时指定的毫秒数
        /// </summary>
        /// <param name="m">毫秒</param>
        public static void TickTimeDelay(int m)
        {
            int firstTickCount, aCount;

            firstTickCount = GetTickCount();
            do
            {
                Application.DoEvents();
                aCount = GetTickCount();
                if (aCount < firstTickCount)
                {
                    if ((2147483647 + aCount) + (2147483647 - firstTickCount) + 2 > m)
                    {
                        return;
                    }
                }
            }
            while (aCount - firstTickCount < m);
        }

        /// <summary>
        /// 判断从起始时刻开始等待的毫秒数是否已到时
        /// </summary>
        /// <param name="firstTick">起始时刻</param>
        /// <param name="waitlong">要等待的毫秒数</param>
        /// <returns></returns>
        public static bool TickTimeIsArrived(int firstTick, int waitlong)
        {
            int aCount = GetTickCount();
            if (aCount < firstTick)
            {
                if (aCount < 0 && firstTick > 0)
                {
                    //只有这时才认为Windows计时由正变负，才这样比较
                    if ((2147483647 + aCount) + (2147483647 - firstTick) + 2 >= waitlong)
                        return true;
                    else
                        return false;
                }
                else
                {
                    //若两者都>0或都<0，则认为Windows计时错误，直接结束等待
                    return true;
                }
            }
            else
            {
                if (aCount - firstTick >= waitlong)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// 计算两个Tick之间的差
        /// </summary>
        /// <param name="firstTick">起始时间</param>
        /// <param name="secondTick">结束时间</param>
        /// <returns></returns>
        public static int TickTimeDifference(int firstTick, int secondTick)
        {
            if (firstTick > 0 && secondTick < 0)
            {
                //只有这时才认为Windows计时由正变负
                return (2147483647 + secondTick) + (2147483647 - firstTick) + 2;
            }
            else
            {
                return secondTick - firstTick;
            }
        }

        /// <summary>
        /// 取本日0:00的时间
        /// </summary>
        /// <returns></returns>
        public static DateTime GetTodayZero()
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        }

        /// <summary>
        /// 取本日0:00的时间
        /// </summary>
        /// <param name="t">指定时间</param>
        /// <returns></returns>
        public static DateTime GetTodayZero(DateTime t)
        {
            return new DateTime(t.Year, t.Month, t.Day, 0, 0, 0);
        }

        /// <summary>
        /// 取昨日0:00的时间
        /// </summary>
        /// <returns></returns>
        public static DateTime GetYesterdayZero()
        {
            DateTime t = DateTime.Now;
            t = t.AddDays(-1);
            return new DateTime(t.Year, t.Month, t.Day, 0, 0, 0);
        }

        /// <summary>
        /// 从起始时间开始判断等待的时间是否已到
        /// </summary>
        /// <param name="firstTime">起始时间</param>
        /// <param name="waitTime">需要等待的时间</param>
        /// <param name="timeUnit">时间单位</param>
        /// <returns></returns>
        public static bool TimeIsArrived(DateTime firstTime, double waitTime, TimeUnit timeUnit)
        {
            TimeSpan ts = DateTime.Now - firstTime;
            switch (timeUnit)
            {
                case TimeUnit.Second:
                    if (ts.TotalSeconds >= waitTime) return true;
                    break;
                case TimeUnit.Minute:
                    if (ts.TotalMinutes >= waitTime) return true;
                    break;
                case TimeUnit.Hour:
                    if (ts.TotalHours >= waitTime) return true;
                    break;
                default:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 计算两个Time之间的差
        /// </summary>
        /// <param name="firstTime">起始时间</param>
        /// <param name="secondTime">结束时间</param>
        /// <param name="timeUnit">时间单位</param>
        /// <returns></returns>
        public static double TimeDifference(DateTime firstTime, DateTime secondTime, TimeUnit timeUnit)
        {
            TimeSpan ts = secondTime - firstTime;
            switch (timeUnit)
            {
                case TimeUnit.Second:
                    return ts.TotalSeconds;
                case TimeUnit.Minute:
                    return ts.TotalMinutes;
                case TimeUnit.Hour:
                    return ts.TotalHours;
                default:
                    break;
            }
            return 0;
        }

        /// <summary>
        /// 取得当前小时的整点时刻
        /// </summary>
        /// <returns></returns>
        public static DateTime GetBeforeHour()
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
        }
    }

}
