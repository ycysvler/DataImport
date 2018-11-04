using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

namespace HyUtilities
{
    /// <summary>
    /// ��ʱ�ļ�ʱ��λ
    /// </summary>
    public enum TimeUnit : int
    {
        /// <summary>
        /// �����ʱ
        /// </summary>
        Second = 0,
        /// <summary>
        /// ���ּ�ʱ
        /// </summary>
        Minute = 1,
        /// <summary>
        /// ��Сʱ��ʱ
        /// </summary>
        Hour = 2,
    }

    public class HyTick
    {
        [DllImport("kernel32.dll")]
        public static extern int GetTickCount();

        /// <summary>
        /// ȡ�ú�����
        /// </summary>
        /// <returns></returns>
        public static int TickTimeGet()
        {
            return GetTickCount();
        }

        /// <summary>
        /// ��ʱָ���ĺ�����
        /// </summary>
        /// <param name="m">����</param>
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
        /// �жϴ���ʼʱ�̿�ʼ�ȴ��ĺ������Ƿ��ѵ�ʱ
        /// </summary>
        /// <param name="firstTick">��ʼʱ��</param>
        /// <param name="waitlong">Ҫ�ȴ��ĺ�����</param>
        /// <returns></returns>
        public static bool TickTimeIsArrived(int firstTick, int waitlong)
        {
            int aCount = GetTickCount();
            if (aCount < firstTick)
            {
                if (aCount < 0 && firstTick > 0)
                {
                    //ֻ����ʱ����ΪWindows��ʱ�����为���������Ƚ�
                    if ((2147483647 + aCount) + (2147483647 - firstTick) + 2 >= waitlong)
                        return true;
                    else
                        return false;
                }
                else
                {
                    //�����߶�>0��<0������ΪWindows��ʱ����ֱ�ӽ����ȴ�
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
        /// ��������Tick֮��Ĳ�
        /// </summary>
        /// <param name="firstTick">��ʼʱ��</param>
        /// <param name="secondTick">����ʱ��</param>
        /// <returns></returns>
        public static int TickTimeDifference(int firstTick, int secondTick)
        {
            if (firstTick > 0 && secondTick < 0)
            {
                //ֻ����ʱ����ΪWindows��ʱ�����为
                return (2147483647 + secondTick) + (2147483647 - firstTick) + 2;
            }
            else
            {
                return secondTick - firstTick;
            }
        }

        /// <summary>
        /// ȡ����0:00��ʱ��
        /// </summary>
        /// <returns></returns>
        public static DateTime GetTodayZero()
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        }

        /// <summary>
        /// ȡ����0:00��ʱ��
        /// </summary>
        /// <param name="t">ָ��ʱ��</param>
        /// <returns></returns>
        public static DateTime GetTodayZero(DateTime t)
        {
            return new DateTime(t.Year, t.Month, t.Day, 0, 0, 0);
        }

        /// <summary>
        /// ȡ����0:00��ʱ��
        /// </summary>
        /// <returns></returns>
        public static DateTime GetYesterdayZero()
        {
            DateTime t = DateTime.Now;
            t = t.AddDays(-1);
            return new DateTime(t.Year, t.Month, t.Day, 0, 0, 0);
        }

        /// <summary>
        /// ����ʼʱ�俪ʼ�жϵȴ���ʱ���Ƿ��ѵ�
        /// </summary>
        /// <param name="firstTime">��ʼʱ��</param>
        /// <param name="waitTime">��Ҫ�ȴ���ʱ��</param>
        /// <param name="timeUnit">ʱ�䵥λ</param>
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
        /// ��������Time֮��Ĳ�
        /// </summary>
        /// <param name="firstTime">��ʼʱ��</param>
        /// <param name="secondTime">����ʱ��</param>
        /// <param name="timeUnit">ʱ�䵥λ</param>
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
        /// ȡ�õ�ǰСʱ������ʱ��
        /// </summary>
        /// <returns></returns>
        public static DateTime GetBeforeHour()
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
        }
    }

}
