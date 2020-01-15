using System.Collections;

namespace GestureKit.Core
{
    public interface IHierarhy
    {
        object GetParent(object target);
        IEnumerable GetChildren(object target);
    }
}