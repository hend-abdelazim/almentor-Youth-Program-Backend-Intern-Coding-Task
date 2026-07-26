namespace TaskManagement.Application.Exceptions;

public class DuplicateEntityException : Exception
{
    public DuplicateEntityException()
    {
    }

    public DuplicateEntityException(string message)
        : base(message)
    {
    }

    public DuplicateEntityException(string entityName, string fieldName, string fieldValue)
        : base($"{entityName} with {fieldName} '{fieldValue}' already exists.")
    {
    }
}
