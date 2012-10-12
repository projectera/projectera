using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ERAUtils.Logger;

namespace ProjectERA.Data.Update
{
    public class Change<valType> : IApplyable
    {
        private valType _value;
        private Object _target;
        private PropertyInfo _property;
        
        /// <summary>
        /// Creates a new Change, used to queue changes to be made to the data values of classes.
        /// </summary>
        /// <param name="target">The class where the property resides.</param>
        /// <param name="property">The name of the property to be changed.</param>
        /// <param name="value">The new value of the property</param>
        internal Change(Object target, String property, valType value)
        {
            _target = target;
            _property = target.GetType().GetProperty(property);
            _value = value;

            if (_property == null)
                Logger.Warning(new String[] { "Property (p:", property, ") for type (t:", target.GetType().Name, ") is not available." });
        }

        /// <summary>
        /// Integrates the change in the specified target
        /// </summary>
        public void Apply()
        {
            if (_property != null)
                _property.SetValue(_target, _value, null);
        }
    }

    public class Change : IApplyable
    {
        private Object _value;
        private Object _target;
        private PropertyInfo _property;

        /// <summary>
        /// Creates a new Change, used to queue changes to be made to the data values of classes.
        /// </summary>
        /// <param name="target">The class where the property resides.</param>
        /// <param name="property">The name of the property to be changed.</param>
        /// <param name="value">The new value of the property</param>
        internal Change(Object target, String property, Object value)
        {
            _target = target;
            _property = target.GetType().GetProperty(property);
            _value = value;
        }

        /// <summary>
        /// Integrates the change in the specified target
        /// </summary>
        public void Apply()
        {
            if (_property != null)
                _property.SetValue(_target, _value, null);
        }
    }

    public class ApplyableAction : IApplyable
    {
        Action action;

        internal ApplyableAction(Action action)
        {
            this.action = action;
        }

        public void Apply()
        {
            if (this.action != null)
                this.action.Invoke();
        }
    }
}
