namespace ObjectiveXML.Models
{
    using System.Dynamic;

    internal class DynamicSerializationModel : DynamicModel
    {
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            object oldValue;

            InnerData.TryGetValue(binder.Name, out oldValue);

            if (oldValue != value)
            {
                InnerData[binder.Name] = value;
            }

            return true;
        }
    }
}
