using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Consoles
{
    public class WriteLog100W
    {
        public static void Write() {

            string filename = "data.txt";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename, true))
            {
                file.WriteLine("时间,数据1,数据2,数据3,数据4,数据5,数据6,数据7,数据8,数据9");

                for (int i = 1; i <= 10 * 10000; i++) {
                    file.WriteLine("{0},数据1-{0},数据2-{0},数据3-{0},数据4-{0},数据5-{0},数据6-{0},数据7-{0},数据8-{0},数据9-{0}", i);
                }  
            }


        }
    }
}
