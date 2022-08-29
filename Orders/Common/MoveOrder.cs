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
        private ICollection<RouteOrder> ListRouteOrder;
        private readonly ICollection<RouteOrder> RootListRouteOrder;


        public MoveOrder(Order ord, ICollection<RouteOrder> listStep, EnumAction act, RouteOrder CurStep, RouteOrder ToStep = null  )
        {
            CurrentStep = CurStep;
            NextStep = ToStep;
            action = act;
            order = ord;
            RootListRouteOrder = listStep;

            if (CurrentStep.ro_parentId != null)
                ListRouteOrder = CurrentStep.ParentRouteOrder.ChildRoutes;
            else
                ListRouteOrder = listStep;
        }

        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        public void MoveToNextStep(ICollection<RouteAdding> ListFiles)
        {
            CurrentStep.RouteAddings = ListFiles;
            RepositoryFiles repoFiles = new RepositoryFiles();
            repoFiles.AddFilesAsync(CurrentStep);
            ICollection<RouteOrder> SelectRoute = null;
            IEnumerable<RouteOrder> ListNextStep = null;
            bool IsNextChild = false;
            bool IsUp = false;

            switch (action)
            {
                case EnumAction.Return:
                    // промежуточные этапы
                    foreach (var item in RootListRouteOrder)
                    {
                        if(NextStep is null)
                            throw new ArgumentNullException(nameof(NextStep));

                        // у промежуточных этапов удаляем статус
                        if (item.ro_step >= NextStep.ro_step && item.ro_step < CurrentStep.ro_step)
                        {
                            item.ro_check = EnumCheckedStatus.CheckedNone;
                            item.ro_statusId = EnumStatus.None;
                            item.ro_date_check = null;
                            foreach(var child in item.ChildRoutes)
                            {
                                child.ro_check = EnumCheckedStatus.CheckedNone;
                                child.ro_statusId = EnumStatus.None;
                                child.ro_date_check = null;
                            }
                        }
                    }

                    SelectRoute = RootListRouteOrder;
                    break;

                case EnumAction.Send:

                    // проверка на непройденные дочерние этапы
                    IsNextChild = CurrentStep.ChildRoutes.Any(it => it.ro_check == EnumCheckedStatus.CheckedNone);

                    SelectRoute = IsNextChild ? CurrentStep.ChildRoutes : ListRouteOrder;

                    // следующий этап
                    NextStep = SelectRoute.FirstOrDefault(item => item.ro_check == EnumCheckedStatus.CheckedNone);

                    if(NextStep == null)
                    {
                        // все этапы пройдены
                        if (CurrentStep.ro_parentId != null)
                        {
                            // возвращаемся выше
                            SelectRoute = RootListRouteOrder;
                            NextStep = SelectRoute.FirstOrDefault(item => item.ro_check == EnumCheckedStatus.CheckedNone);
                            IsUp = true;
                        }
                    }

                    break;
            }
            // изменяем статус текущего шага
            SetStatusStep(IsNextChild);

            // количество выполняющихся параллельных этапов
            int CountSameStep = SelectRoute.Count(item => item.ro_step == CurrentStep.ro_step
                        && (item.ro_check == EnumCheckedStatus.CheckedProcess || item.ro_statusId == EnumStatus.Waiting));

            // если переход на подчиненную ветку или нет параллельного этапа
            if (IsNextChild || CountSameStep == 0 || IsUp)
            {
                // получаем все следующие шаги
                ListNextStep = SelectRoute.Where(it => it.ro_step == NextStep.ro_step
                    && it.ro_check == EnumCheckedStatus.CheckedNone);

                foreach (var item in ListNextStep)
                    SetStatusNextStep(item);
            }

            if (NextStep is null) NextStep = CurrentStep;

            order.o_statusId = NextStep.ro_statusId;
            order.o_stepRoute = NextStep.ro_step;

            ShareFunction.SendMail(NextStep.User.u_email, order.o_number);

        }

        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        private void SetStatusNextStep(RouteOrder NextStep)
        {
                // если следующий шаг с возвратом, то изменяем статусы текущего шага
                NextStep.ro_check = EnumCheckedStatus.CheckedProcess;

                switch ((EnumTypesStep)NextStep.ro_typeId)
                {
                    case EnumTypesStep.Coordinate:
                        NextStep.ro_statusId = EnumStatus.CoordinateWork;
                        break;

                    case EnumTypesStep.Approve:
                        NextStep.ro_statusId = EnumStatus.ApprovWork;
                        break;

                    case EnumTypesStep.Review:
                        NextStep.ro_statusId = EnumStatus.CoordinateWork;
                        break;

                    case EnumTypesStep.Notify:
                        NextStep.ro_statusId = EnumStatus.CoordinateWork;
                        break;

                    case EnumTypesStep.Created:
                        NextStep.ro_statusId = EnumStatus.Created;
                        break;

                }

        }

        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        private void SetStatusStep(bool IsChild)
        {

            switch (action)
            {
                // действие возврата
                case EnumAction.Return:
                    CurrentStep.ro_date_check = DateTime.Now;
                    CurrentStep.ro_statusId = EnumStatus.Return;
                    CurrentStep.ro_check = EnumCheckedStatus.CheckedNone;
                    NextStep.ro_statusId = EnumStatus.CoordinateWork;
                    NextStep.ro_check = EnumCheckedStatus.CheckedProcess;
                    break;


                // действие отправки на следующий этап
                case EnumAction.Send:

                    if(IsChild)
                    {
                        // передача в подуровень
                        CurrentStep.ro_statusId = EnumStatus.Waiting;
                        CurrentStep.ro_check = EnumCheckedStatus.CheckedNone;
                    }
                    else
                    {
                        // если нет передачи на дочерний уровень
                        CurrentStep.ro_date_check = DateTime.Now;
                        CurrentStep.ro_check = EnumCheckedStatus.Checked;

                        switch ((EnumTypesStep)CurrentStep.ro_typeId)
                        {
                            case EnumTypesStep.Coordinate:
                                CurrentStep.ro_statusId = EnumStatus.Coordinated;
                                break;

                            case EnumTypesStep.Approve:
                                CurrentStep.ro_statusId = EnumStatus.Approved;
                                break;

                            case EnumTypesStep.Review:
                                CurrentStep.ro_statusId = EnumStatus.Coordinated;
                                break;

                            case EnumTypesStep.Notify:
                                CurrentStep.ro_statusId = EnumStatus.Coordinated;
                                break;

                            case EnumTypesStep.Created:
                                CurrentStep.ro_statusId = EnumStatus.Created;
                                break;

                        }
                    }
                    break;


                default:
                    CurrentStep.ro_statusId = EnumStatus.None;
                    CurrentStep.ro_check = EnumCheckedStatus.CheckedNone;
                    CurrentStep.ro_date_check = null;
                    break;
            }

        }

    }
}
