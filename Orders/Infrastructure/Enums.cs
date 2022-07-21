namespace Orders.Infrastructure
{
    public enum EnumStatus : int { Created = 1, CoordinateWork, Coordinated, ApprovWork,  Approved, Return, Refused, Closed, Waiting };
    public enum EnumTypesStep : int { Coordinate = 1,  Approve, Review, Notify };
    public enum EnumAction : int { Send, Return, Refuse, Close};
}