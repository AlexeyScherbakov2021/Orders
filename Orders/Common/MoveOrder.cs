using Orders.Infrastructure;
using Orders.Infrastructure.Common;
using Orders.Models;
using Orders.Repository;
using Orders.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
            bool AllCheckStep = false;

            switch (action)
            {
                case EnumAction.Return:

                    if (NextStep is null)
                        throw new ArgumentNullException(nameof(NextStep));

                    // сброс статусов для подчиненного маршрута
                    foreach (var child in CurrentStep.ChildRoutes)
                    {
                        child.ro_check = EnumCheckedStatus.CheckedNone;
                        child.ro_statusId = EnumStatus.None;
                        child.ro_date_check = null;
                    }

                    // промежуточные этапы
                    foreach (var item in RootListRouteOrder)
                    {

                        // у промежуточных этапов удаляем статус
                        if (item.ro_step > NextStep.ro_step && item.ro_step < CurrentStep.ro_step)
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

                    // изменяем статус текущего шага
                    SetStatusStep(IsNextChild);

                    ListNextStep = RootListRouteOrder.Where(it => it.ro_step == NextStep.ro_step
                        && it.ro_check == EnumCheckedStatus.Checked);

                    if (ListNextStep.Count() > 1
                        && MessageBox.Show("Вернуть на выбранный этап или на все?\n" +
                        "Да - на выбранный.\n" +
                        "Нет - на все.", "Вопрос", 
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        ListNextStep = new RouteOrder[] { NextStep };
                    }

                    // обновляем статусы для следующих этапов
                    foreach (var item in ListNextStep)
                    {
                        SetStatusNextStep(item);
                        ShareFunction.SendMail(item.User.u_email, order.o_number);
                    }

                    order.o_statusId = NextStep.ro_statusId;
                    order.o_stepRoute = NextStep.ro_step;
                    order.o_LastTimeStep = CurrentStep.ro_date_check;
                    return;


                case EnumAction.Send:

                    // проверка на непройденные дочерние этапы
                    IsNextChild = CurrentStep.ChildRoutes.Any(it => it.ro_check == EnumCheckedStatus.CheckedNone);

                    SelectRoute = IsNextChild ? CurrentStep.ChildRoutes : ListRouteOrder;

                    // изменяем статус текущего шага
                    SetStatusStep(IsNextChild);

                    // есть ли в этой ветке этапы на рассмотрении
                    AllCheckStep = SelectRoute.All(item => item.ro_check == EnumCheckedStatus.Checked);
                    if (AllCheckStep)      // есть этапы на рассмотрении
                    {
                        // все этапы пройдены
                        if (CurrentStep.ro_parentId != null)
                        {
                            // возвращаемся на маршрут выше
                            SelectRoute = RootListRouteOrder;
                            NextStep = SelectRoute.FirstOrDefault(item => item.ro_check == EnumCheckedStatus.CheckedNone);
                            IsUp = true;
                        }
                        break;
                    }

                    // следующий этап
                    NextStep = SelectRoute.FirstOrDefault(item => item.ro_check == EnumCheckedStatus.CheckedNone);

                    if (NextStep == null)
                        NextStep = SelectRoute.FirstOrDefault(item => item.ro_check == EnumCheckedStatus.CheckedProcess);

                    break;

            }

            // количество выполняющихся параллельных этапов в текущий момент
            int CountSameStep = MainWindowViewModel.repo.RouteOrders.AsNoTracking()
                .Count(it => it.ro_step == CurrentStep.ro_step
                        && it.ro_parentId == CurrentStep.ro_parentId
                        && it.ro_orderId == CurrentStep.ro_orderId
                        && (it.ro_check == EnumCheckedStatus.CheckedProcess || it.ro_statusId == EnumStatus.Waiting));


            //int CountSameStep = SelectRoute.Count(item => item.ro_step == CurrentStep.ro_step
            //            && (item.ro_check == EnumCheckedStatus.CheckedProcess || item.ro_statusId == EnumStatus.Waiting));

            if (NextStep is null)
            {
                // все этапы завершены, этапов больше нет
                //NextStep = CurrentStep;
                // отправка оповещения инициатору
                User Owner = MainWindowViewModel.repo.Users.FirstOrDefault(it => it.id == CurrentStep.ro_ownerId);
                ShareFunction.SendMail(Owner.u_email, order.o_number);
                order.o_statusId = CurrentStep.ro_statusId;
                order.o_stepRoute = CurrentStep.ro_step;
                order.o_LastTimeStep = CurrentStep.ro_date_check;
            }
            else
            {
                // если переход на подчиненную ветку или нет параллельного этапа
                // IsNextChild - будет переход в дочернюю ветку
                // IsUp - возврат из дочерней ветки
                if (IsNextChild || CountSameStep <= 1 || IsUp)
                {
                    // получаем все следующие шаги
                    ListNextStep = SelectRoute.Where(it => it.ro_step == NextStep.ro_step
                        && it.ro_check == EnumCheckedStatus.CheckedNone);

                    // обновляем статусы для следующих этапов
                    foreach (var item in ListNextStep)
                    {
                        SetStatusNextStep(item);
                        ShareFunction.SendMail(item.User.u_email, order.o_number);
                    }

                    order.o_statusId = NextStep.ro_statusId;
                    order.o_stepRoute = NextStep.ro_step;
                    order.o_LastTimeStep = CurrentStep.ro_date_check;
                }
            }


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
                    //NextStep.ro_statusId = EnumStatus.CoordinateWork;
                    //NextStep.ro_check = EnumCheckedStatus.CheckedProcess;
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
