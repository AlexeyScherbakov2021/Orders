using Orders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Common
{
    internal class WrapStep : RouteOrder
    {
        //public RouteOrder Step;               // сам этап
        public WrapStep NextStep;               // следующий этап
        public WrapStep ParentStep;             // родительский этап
        public ICollection<WrapStep> Child;


        public string GetNumber()               // получение собранного номера этапа
        {
            string num = "";

            while(ParentStep != null)
                num = ParentStep.GetNumber() + "." + num;
            return  (num == "" ? "" : (num + "."))  + ro_step.ToString();
        }

    }



}
