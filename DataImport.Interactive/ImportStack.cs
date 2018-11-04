using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DataImport.Interactive
{
    public class ImportStack
    {
        private static Stack<UIElement> stack = new Stack<UIElement>();

        public static void clear() {
            stack.Clear();            
        }

        public static UIElement Pop()
        {
            return stack.Pop();
        }

        public static void Push(UIElement item) {
            stack.Push(item);
        }
    }
}
