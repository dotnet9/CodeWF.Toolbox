namespace Tools.CodeWF.Fluent.Validation;

public interface IRegisterValidationMethod
{
	void RegisterValidationMethod(string propertyName, ValidateMethod validateMethod);
}
