StructuredBuffer<int> _propertyPointers;

void PropertyPointer_float(in float instanceID, out float index)
{
    index = _propertyPointers[instanceID];
}