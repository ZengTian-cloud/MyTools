using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CoreObject
{
    public int Id { get; set; }
    public bool Active { get; set; }
    public bool Recycled { get; set; }

    protected virtual void OnUpdate() { }
    protected virtual void OnRecycle() { }
    protected virtual void OnStart() { }

    public void Update() { OnUpdate(); }
    public void Start() { OnStart(); }
    public void Recycle() { OnRecycle(); }

    public void Destroy()
    {
        //EventManager.Instance.Destroy(this);
        // CoreObjectManager.Instance.Recycle(this); }
    }

}