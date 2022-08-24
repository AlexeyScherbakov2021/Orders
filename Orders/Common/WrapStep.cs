using Orders.Infrastructure.Converters;
using Orders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Common
{
    internal class WrapStep 
    {
        public RouteOrder Step { get; set; }            // сам этап
        public WrapStep NextStep;                       // следующий этап
        public WrapStep ParentStep;                     // родительский этап
        public IEnumerable<WrapStep> Child;


        public WrapStep(RouteOrder ro)
        {
            Step = ro;
        }

        public string GetNumber()               // получение собранного номера этапа
        {
            string num = "";

            while (ParentStep != null)
                num = ParentStep.GetNumber() + "." + num;
            return (num == "" ? "" : (num + ".")) + Step.ro_step.ToString();
        }



        public void InsertStep(WrapStep step)
        {
            step.NextStep = NextStep;
            NextStep = step;
        }



        public static ICollection<WrapStep> RouteOrderToWrapStep(ICollection<RouteOrder> listStep)
        {
            List<WrapStep> listWrap = new List<WrapStep>();

            WrapStep prevStep = null;

            foreach (RouteOrder routeOrder in listStep)
            {
                WrapStep wrapStep = new WrapStep(routeOrder);
                listWrap.Add(wrapStep);
                if(prevStep != null)
                    prevStep.NextStep = wrapStep;
                prevStep = wrapStep;
            }

            // внедрение дочерних этапов
            foreach(WrapStep wrapStep in listWrap)
            {
                wrapStep.Child = listWrap.Where(it => it.Step.ro_return_step == wrapStep.Step.ro_step);
                foreach (var child in wrapStep.Child)
                    child.ParentStep = wrapStep;
            }

            IEnumerable<WrapStep> listChild = listWrap.Where(it => it.ParentStep != null);
            foreach (var item in listChild)
                listWrap.Remove(item);

            return listWrap;
        }


    }

}
