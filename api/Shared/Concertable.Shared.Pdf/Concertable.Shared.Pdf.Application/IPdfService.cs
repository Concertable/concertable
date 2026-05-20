using QuestPDF.Infrastructure;

namespace Concertable.Shared.Pdf;

public interface IPdfService
{
    byte[] Render(IDocument document);
}
