using System.IO;

namespace ObjLoader.Loader.Loaders
{
    using System.Collections;

    public abstract class LoaderBase
    {
        private StreamReader _lineStreamReader;

        protected void StartLoad(Stream lineStream)
        {
            _lineStreamReader = new StreamReader(lineStream);

            while (!_lineStreamReader.EndOfStream)
            {
                ParseLine();
            }
        }

        protected IEnumerator StartLoadAsync(Stream lineStream)
        {
            _lineStreamReader = new StreamReader(lineStream);

            while (!_lineStreamReader.EndOfStream)
            {
                ParseLine();
                yield return _lineStreamReader.BaseStream.Position / _lineStreamReader.BaseStream.Length;
            }
        }

        private bool IsNullOrWhiteSpace(string value)
        {
            if (value == null)
            {
                return true;
            }

            for (var i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private void ParseLine()
        {
            var currentLine = _lineStreamReader.ReadLine();

            // handles obj files generated by PrimitivesPro v1.8.3 http://u3d.as/4gQ
            if (!this.IsNullOrWhiteSpace(currentLine))
            {
                var clone = new string(currentLine[0], currentLine.Length);
                if (clone == currentLine)
                {
                    return;
                }
            }

            if (this.IsNullOrWhiteSpace(currentLine) || currentLine[0] == '#')
            {
                return;
            }

            var fields = currentLine.Trim().Split(null, 2);
            var keyword = fields[0].Trim();
            var data = fields[1].Trim();

            ParseLine(keyword, data);
        }

        protected abstract void ParseLine(string keyword, string data);
    }
}