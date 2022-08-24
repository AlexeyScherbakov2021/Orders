using Orders.Infrastructure;
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
        //public WrapStep NextStep;                       // следующий этап
        public WrapStep ParentStep;                     // родительский этап
        public RouteSteps Child;
        public bool IsWait;
        public RouteSteps OwnerRoute;

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
            //step.NextStep = NextStep;
            //NextStep = step;
        }


        public void SetCurrentStep()
        {
            Step.ro_check = EnumCheckedStatus.CheckedProcess;
        }

        public void SetCompleteStep()
        {
            Step.ro_check = EnumCheckedStatus.Checked;
        }

        public void SetWaitStep()
        {
            Step.ro_check = EnumCheckedStatus.CheckedNone;
            Step.ro_statusId = (int)EnumStatus.Waiting;
        }



        public static ICollection<WrapStep> RouteOrderToWrapStep(ICollection<RouteOrder> listStep)
        {
            List<WrapStep> listWrap = new List<WrapStep>();

            WrapStep prevStep = null;

            foreach (RouteOrder routeOrder in listStep)
            {
                WrapStep wrapStep = new WrapStep(routeOrder);
                listWrap.Add(wrapStep);
                //if(prevStep != null)
                //    prevStep.NextStep = wrapStep;
                prevStep = wrapStep;
            }

            // внедрение дочерних этапов
            //foreach(WrapStep wrapStep in listWrap)
            //{
            //    wrapStep.Child = listWrap.Where(it => it.Step.ro_return_step == wrapStep.Step.ro_step);
            //    foreach (var child in wrapStep.Child)
            //        child.ParentStep = wrapStep;
            //}

            IEnumerable<WrapStep> listChild = listWrap.Where(it => it.ParentStep != null);
            foreach (var item in listChild)
                listWrap.Remove(item);

            return listWrap;
        }


    }

    internal class RouteSteps
    {
        public IEnumerable<WrapStep> ListWrap;
        //public WrapStep CurrentStep;
        //public WrapStep NextStep;
        public bool IsComplete => ListWrap.All(it => it.Step.ro_check == EnumCheckedStatus.Checked);
        public bool IsParallel;
        public WrapStep ReturnStep;


        public RouteSteps(IEnumerable<RouteOrder> listStep)
        {
            List<WrapStep> listWrap = new List<WrapStep>();

            WrapStep prevStep = null;

            foreach (RouteOrder routeOrder in listStep)
            {
                WrapStep wrapStep = new WrapStep(routeOrder);
                if(routeOrder.ro_return_step != null && prevStep?.Child is null && prevStep != null)
                {
                    IEnumerable<RouteOrder> child = listStep.Where(it => it.ro_return_step == routeOrder.ro_return_step);
                    prevStep.Child = new RouteSteps(child);

                }
                else
                    listWrap.Add(wrapStep);
                prevStep = wrapStep;
            }

            // внедрение дочерних этапов
            //foreach (WrapStep wrapStep in listWrap)
            //{
            //    wrapStep.Child = listWrap.Where(it => it.Step.ro_return_step == wrapStep.Step.ro_step);
            //    foreach (var child in wrapStep.Child)
            //        child.ParentStep = wrapStep;
            //}

            //IEnumerable<WrapStep> listChild = listWrap.Where(it => it.ParentStep != null);
            //foreach (var item in listChild)
            //    listWrap.Remove(item);

            this.ListWrap = listWrap;
        }


        public void NextStepMove()
        {
            //if(CurrentStep.Child != null && !CurrentStep.Child.IsComplete)
            //{
            //    if(!CurrentStep.Child.IsParallel)
            //        CurrentStep.SetWaitStep();
            //    else
            //        CurrentStep.SetCompleteStep();

            //    CurrentStep.Child.StartStep();
            //    return;
            //}

            //CurrentStep.SetCompleteStep();
            //if (IsComplete)
            //{
            //    ReturnStep.SetCurrentStep();
            //    //ParentStep.Step.ro_check = EnumCheckedStatus.Checked;
            //    return;
            //}

            //if(!IsParallel)
            //    NextStep.SetCurrentStep();

        }

        public void StartStep()
        {
            if(IsParallel)
            {
                foreach(var item in ListWrap)
                    item.SetCurrentStep();
            }
            else
                ListWrap.First().SetCurrentStep();
        }


    }

}
