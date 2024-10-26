using System;

namespace Xabbo.Exceptions;

public class PlacementNotFoundException() : Exception("No valid placement tiles found.") { }