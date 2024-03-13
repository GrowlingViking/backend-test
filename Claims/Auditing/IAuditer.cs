namespace Claims.Auditing;

public interface IAuditer
{
    public void AuditClaim(string id, string httpRequestType);
    public void AuditCover(string id, string httpRequestType);
}
