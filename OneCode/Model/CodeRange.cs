namespace OneCode {
    class CodeRange {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public CodeRange(int _startIndex, int _endIndex) {
            StartIndex = _startIndex;
            EndIndex = _endIndex;
        }
    }
}
