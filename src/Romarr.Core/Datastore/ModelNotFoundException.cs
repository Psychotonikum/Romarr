using System;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Datastore
{
    public class ModelNotFoundException : RomarrException
    {
        public ModelNotFoundException(Type modelType, int modelId)
            : base("{0} with ID {1} does not exist", modelType.Name, modelId)
        {
        }
    }
}
