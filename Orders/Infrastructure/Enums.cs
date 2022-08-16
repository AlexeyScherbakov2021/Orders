namespace Orders.Infrastructure
{
    public enum EnumStatus : int { Created = 1, CoordinateWork, Coordinated, ApprovWork,  Approved, Return, Refused, Closed, Waiting, None };
    public enum EnumTypesStep : int { Coordinate = 1,  Approve, Review, Notify, Created };
    public enum EnumAction : int { Send, Return, Refuse, Close};
    public enum EnumCheckedStatus : int { CheckedNone = 0, CheckedProcess,  Checked };
}