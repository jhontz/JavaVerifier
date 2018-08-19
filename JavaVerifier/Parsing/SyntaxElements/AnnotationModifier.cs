namespace JavaVerifier.Parsing.SyntaxElements {

  internal sealed class AnnotationModifier : Modifier {
    public Annotation Annotation { get; }

    public AnnotationModifier(Annotation annotation) {
      Annotation = annotation;
    }

  }

}
