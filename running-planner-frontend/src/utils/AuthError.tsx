export const handleAuthError = async (
    response: Response,
    setErrorMessage: React.Dispatch<React.SetStateAction<string | null>>,
    action: string
): Promise<boolean> => {
    if (response.status === 401) {
        const errorData = await response.json();
        setErrorMessage(errorData.message + `. Tried: ${action}.`|| "Unauthorized access.");
        return true;
    } else if (response.status === 403) {
        setErrorMessage(`You do not have to permission ${action}.`);
        return true;
    }
    return false;
};
