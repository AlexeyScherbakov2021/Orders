using Orders.Infrastructure;
using Orders.Infrastructure.Common;
using Orders.Models;
using Orders.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;

namespace Orders.Common
{
    internal class MoveOrder
    {
        private readonly RouteOrder CurrentStep;
        private List<RouteOrder> NextSteps;
        private readonly Order order;
        private readonly EnumAction action;


        public MoveOrder(Order ord, EnumAction act, RouteOrder CurStep, RouteOrder ToStep = null  )
        {
            CurrentStep = CurStep;
            NextSteps = new List<RouteOrder>();
            if(ToStep != null)
                NextSteps.Add(ToStep);
            action = act;
            order = ord;
        }

        //------------------------------------------------------------------------------------------
        // переход к следующему этапу
        //------------------------------------------------------------------------------------------
        public void MoveToNextStep(ICollection<RouteAdding> ListFiles)
        {
            RouteOrder NextStepOne;
            //IEnumerable<RouteOrder> NextSteps = null;
            bool IsParent = false;

            if (ListFiles != null)
            {
                CurrentStep.RouteAddings = ListFiles;
                RepositoryFiles repoFiles = new RepositoryFiles();
                repoFiles.AddFilesAsync(CurrentStep);
            }

            switch (action)
            {
                case EnumAction.Return:
                    // промежуточные этапы
                    if (NextSteps.Count == 0)
                        throw new ArgumentNullException(nameof(NextSteps));

                    foreach (var item in order.RouteOrders)
                    {
                        // у промежуточных этапов удаляем статус
                        if (item.ro_step > NextSteps.First().ro_step && item.ro_step < CurrentStep.ro_step)
                        {
                            item.ro_check = EnumCheckedStatus.CheckedNone;
                            item.ro_statusId = (int)EnumStatus.None;
                            item.ro_date_check = null;
                        }
                    }

                    break;

                case EnumAction.Send:

                    // если это подчиненная ветка
                    if (CurrentStep.ro_return_step != null)
                    {
                        // если есть другие выполняющиеся, то переход не делаем
                        if (order.RouteOrders.Count(item => item.ro_check == EnumCheckedStatus.CheckedProcess
                            && item.ro_return_step == CurrentStep.ro_return_step) > 1)
                            break;
                        else
                        {
                            // ищем следующий шаг в подветке
                            NextStepOne = order.RouteOrders.FirstOrDefault(item => item.ro_step > CurrentStep.ro_step
                                && item.ro_return_step == CurrentStep.ro_return_step
                                && item.ro_check == EnumCheckedStatus.CheckedNone);

                            if(NextStepOne is null)
                            {
                                // переходим на возвратный шаг
                                NextStepOne = order.RouteOrders.Single(item => item.ro_step == CurrentStep.ro_return_step);
                                NextSteps.Add(NextStepOne);
                                break;
                            }
                            else
                                // находим все следующие этапы
                                NextSteps = order.RouteOrders.Where(item => item.ro_step == NextStepOne.ro_step
                                            && item.ro_return_step == NextStepOne.ro_return_step
                                            && item.ro_check == EnumCheckedStatus.CheckedNone).ToList();

                        }
                    }

                    // это основная ветка
                    else
                    {
                        // если есть выполняющийся этап, то меняем только статус текущего
                        if (order.RouteOrders.Count(item => item.ro_check == EnumCheckedStatus.CheckedProcess) > 1)
                            break;

                        NextStepOne = order.RouteOrders.FirstOrDefault(item => item.ro_step > CurrentStep.ro_step
                            && item.ro_check == EnumCheckedStatus.CheckedNone);

                        // находим все следующие этапы
                        NextSteps = order.RouteOrders.Where(item => item.ro_step == NextStepOne?.ro_step
                                    && item.ro_check == EnumCheckedStatus.CheckedNone).ToList();

                        IsParent = NextSteps.Any(item => item.ro_return_step != null);
                    }

                    break;
            }

            SetStatusStep(IsParent);

            //if (NextStep is null) NextStep = CurrentStep;
            //if (order.RouteOrders.All(item => item.ro_check == EnumCheckedStatus.Checked))
            if (NextSteps.Count == 0)
                NextSteps.Add(CurrentStep);

            if (NextSteps.Count > 0)
            {
                order.o_statusId = NextSteps.First().ro_statusId;
                order.o_stepRoute = NextSteps.First().ro_step;

                foreach(var item in NextSteps)
                    ShareFunction.SendMail(item.User.u_email, order.o_number);
            }
        }

        //------------------------------------------------------------------------------------------
        // установка статусов для текущего этапа
        //------------------------------------------------------------------------------------------
        private void SetStatusStep(bool IsParent)
        {

            switch (action)
            {
                // действие возврата
                case EnumAction.Return:
                    CurrentStep.ro_date_check = DateTime.Now;
                    CurrentStep.ro_statusId = (int)EnumStatus.Return;
                    CurrentStep.ro_check = EnumCheckedStatus.CheckedNone;
                    break;


                // действие отправки на следующий этап
                case EnumAction.Send:

                    if (IsParent)
                    {
                        CurrentStep.ro_date_check = null;
                        CurrentStep.ro_check = EnumCheckedStatus.CheckedNone;
                        CurrentStep.ro_statusId = (int)EnumStatus.Waiting;
                    }
                    else
                    {
                        CurrentStep.ro_date_check = DateTime.Now;
                        CurrentStep.ro_check = EnumCheckedStatus.Checked;

                        switch ((EnumTypesStep)CurrentStep.ro_typeId)
                        {
                            case EnumTypesStep.Coordinate:
                                CurrentStep.ro_statusId = (int)EnumStatus.Coordinated;
                                break;
    
                            case EnumTypesStep.Approve:
                                CurrentStep.ro_statusId = (int)EnumStatus.Approved;
                                break;
    
                            case EnumTypesStep.Review:
                                CurrentStep.ro_statusId = (int)EnumStatus.Coordinated;
                                break;
    
                            case EnumTypesStep.Notify:
                                CurrentStep.ro_statusId = (int)EnumStatus.Coordinated;
                                break;
    
                            case EnumTypesStep.Created:
                                CurrentStep.ro_statusId = (int)EnumStatus.Created;
                                break;
    
                        }
                    }

                    break; // case EnumAction.Send


                default:
                    CurrentStep.ro_statusId = (int)EnumStatus.None;
                    CurrentStep.ro_check = EnumCheckedStatus.CheckedNone;
                    CurrentStep.ro_date_check = null;
                    return;
            }

            foreach (var item in NextSteps)
                SetStatusNextStep(item);

        }

        //------------------------------------------------------------------------------------------
        // установка статуса для следующих этапов
        //------------------------------------------------------------------------------------------
        private void SetStatusNextStep(RouteOrder item)
        {
            if (item != null)
            {

                item.ro_check = EnumCheckedStatus.CheckedProcess;

                switch ((EnumTypesStep)item.ro_typeId)
                {
                    case EnumTypesStep.Coordinate:
                        item.ro_statusId = (int)EnumStatus.CoordinateWork;
                        break;

                    case EnumTypesStep.Approve:
                        item.ro_statusId = (int)EnumStatus.ApprovWork;
                        break;

                    case EnumTypesStep.Review:
                        item.ro_statusId = (int)EnumStatus.CoordinateWork;
                        break;

                    case EnumTypesStep.Notify:
                        item.ro_statusId = (int)EnumStatus.CoordinateWork;
                        break;

                    case EnumTypesStep.Created:
                        item.ro_statusId = (int)EnumStatus.Created;
                        break;

                }

            }
        }
    }
}
