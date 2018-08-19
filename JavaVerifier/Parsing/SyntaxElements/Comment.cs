namespace JavaVerifier.Parsing.SyntaxElements {

  internal class Comment : InputElement {
    public string Content { get; }

    public Comment(string content) {
      Content = content;
    }

    public override string ToString() {
      return $"Comment \"{Content}\"";
    }
  }

}
