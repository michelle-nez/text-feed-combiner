// Turns the combined text into a file the browser saves. Blazor Server has no
// direct access to the client file system, so the bytes come over the circuit
// and become a Blob here.
window.saveTextFile = (fileName, text) => {
    const blob = new Blob([text], { type: "text/plain;charset=utf-8" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};
