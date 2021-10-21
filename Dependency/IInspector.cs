using System;

namespace Dependency
{

    public enum InspectorState
    {
        Unknown,    // We are currently not able to determine the state. (Ressource not available, etc.) 
        Blocked,    // We know the state, and it's blocked. We expect this to become available at some point
        Available,  // We know the state, and it's available.
        Expired     // This resource wil never become available
    }
    
    public interface IInspector
    {

        InspectorState Allocate();
        void Regret();
        bool Take();
        void Release();


    }
}