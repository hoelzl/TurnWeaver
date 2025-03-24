using System;
using UnityEngine;

namespace Interaction
{
    // Interactable interface
    public interface IInteractable
    {
        /**
         * Called to get the possible interaction options.
         */
        InteractionOptionSO[] InteractionOptions { get; }

        /**
         * Should we present the option to the user if there is only a single one?
         */
        bool AutoInvokeSingleOption { get; }
    }
}
