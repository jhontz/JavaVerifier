using System;
using System.Collections.Generic;

namespace JavaVerifier {

  internal class ResettableWrapper<T> {

    private Stack<T> _markedData { get; }

    public T Value { get; set; }

    public ResettableWrapper(T value) {
      _markedData = new Stack<T>();
      Value = value;
    }

    public void LookAhead(Action action) {
      _markedData.Push(Value);
      action();
      Value = _markedData.Pop();
    }

  }

}
