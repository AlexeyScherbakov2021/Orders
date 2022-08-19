using Orders.Infrastructure;
using Orders.Infrastructure.Common;
using Orders.Models;
using Orders.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Common
{
    internal class MoveOrder
    {
        private readonly RouteOrder CurrentStep;
        private RouteOrder NextStep;
        private readonly Order order;
        private readonly EnumAction action;


        public MoveOrder(Order ord, EnumAction act, RouteOrder CurStep, RouteOrder ToStep = null  )
        {
            CurrentStep = CurStep;
            NextStep = ToStep;
            action = act;
            order = ord;
        }

        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        public void MoveToNextStep(ICollection<RouteAdding> ListFiles)
        {
            CurrentStep.RouteAddings = ListFiles;
            RepositoryFiles repoFiles = new RepositoryFiles();
            repoFiles.AddFilesAsync(CurrentStep);

            switch (action)
            {
                case EnumAction.Return:
                    // промежуточные этапы
                    foreach (var item in order.RouteOrders)
                    {
                        if(NextStep is null)
                            throw new ArgumentNullException(nameof(NextStep));

                        // у промежуточных этапов удаляем статус
                        if (item.ro_step > NextStep.ro_step && item.ro_step < CurrentStep.ro_step)
                        {
                            item.ro_check = EnumCheckedStatus.CheckedNone;
                            item.ro_statusId = (int)EnumStatus.None;
                            item.ro_date_check = null;
                        }
                    }

                    break;

                case EnumAction.Send:

                    // следующий этап
                    NextStep = order.RouteOrders.FirstOrDefault(item => item.ro_step > CurrentStep.ro_step
                        && item.ro_check != EnumCheckedStatus.Checked);

                    // проверяем на возвратный шаг
                    if(CurrentStep.ro_return_step != null && NextStep?.ro_return_step == null)
                        NextStep = order.RouteOrders.FirstOrDefault(item => item.ro_step == CurrentStep.ro_return_step);

                    break;
            }

            SetStatusStep();

            if (NextStep is null) NextStep = CurrentStep;

            order.o_statusId = NextStep.ro_statusId;
            order.o_stepRoute = NextStep.ro_step;

            ShareFunction.SendMail(NextStep.User.u_email, order.o_number);

        }

        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        private void SetStatusStep()
        {

            switch (action)
            {
                // действие возврата
                case EnumAction.Return:
                    CurrentStep.ro_date_check = DateTime.Now;
                    CurrentStep.ro_statusId = (int)EnumStatus.Return;
                    CurrentStep.ro_check = EnumCheckedStatus.CheckedNone;
                    NextStep.ro_statusId = (int)EnumStatus.CoordinateWork;
                    NextStep.ro_check = EnumCheckedStatus.CheckedProcess;
                    break;


                // действие отправки на следующий этап
                case EnumAction.Send:

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


                    if (NextStep != null)
                    {
                        // если следующий шаг с возвратом, то изменяем статусы текущего шага
                        if(NextStep.ro_return_step != null && CurrentStep.ro_return_step == null)
                        {
                            CurrentStep.ro_date_check =null;
                            CurrentStep.ro_check = EnumCheckedStatus.CheckedNone;
                            CurrentStep.ro_statusId = (int)EnumStatus.Waiting;
                        }

                        NextStep.ro_check = EnumCheckedStatus.CheckedProcess;

                        switch ((EnumTypesStep)NextStep.ro_typeId)
                        {
                            case EnumTypesStep.Coordinate:
                                NextStep.ro_statusId = (int)EnumStatus.CoordinateWork;
                                break;

                            case EnumTypesStep.Approve:
                                NextStep.ro_statusId = (int)EnumStatus.ApprovWork;
                                break;

                            case EnumTypesStep.Review:
                                NextStep.ro_statusId = (int)EnumStatus.CoordinateWork;
                                break;

                            case EnumTypesStep.Notify:
                                NextStep.ro_statusId = (int)EnumStatus.CoordinateWork;
                                break;

                            case EnumTypesStep.Created:
                                NextStep.ro_statusId = (int)EnumStatus.Created;
                                break;

                        }

                    }

                    break; // case EnumAction.Send


                default:
                    CurrentStep.ro_statusId = (int)EnumStatus.None;
                    CurrentStep.ro_check = EnumCheckedStatus.CheckedNone;
                    CurrentStep.ro_date_check = null;
                    break;
            }

        }

    }
}
