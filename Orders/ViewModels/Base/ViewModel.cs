using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Orders.ViewModels.Base
{
    internal class ViewModel : Observable, IValidatableObject, INotifyDataErrorInfo

    {

        //public event PropertyChangedEventHandler PropertyChanged;
        protected readonly Dictionary<string, List<string>> _Errors;
        public bool HasErrors => _Errors.Any();

        public ViewModel()
        {
            _Errors = new Dictionary<string, List<string>>();
        }

        protected void ValidateProperty(string propName, object value)
        {
            var result = new List<ValidationResult>();
            var context = new ValidationContext(this);

            Validator.TryValidateObject(this, context, result, true);

            if (result.Any())
            {
                propName = result[0].MemberNames.First();
                _Errors[propName] = result.Select(r => r.ErrorMessage).Distinct().ToList();
                OnErrorChanged(propName);
            }
            else if (_Errors.ContainsKey(propName))
            {
                _Errors.Remove(propName);
                OnErrorChanged(propName);
            }

        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected virtual void OnErrorChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }


        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }
        public IEnumerable GetErrors(string propertyName)
        {
            return propertyName != null && _Errors.ContainsKey(propertyName)
                ? _Errors[propertyName]
                : Enumerable.Empty<string>();
        }



    }
}
