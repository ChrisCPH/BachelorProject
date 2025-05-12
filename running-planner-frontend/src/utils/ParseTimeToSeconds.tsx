export const ParseTimeToSeconds = (input: string): number => {
    if (!input) return 0;

    if (!input.includes(":")) {
        return Number(input) * 60;
    }

    const [minutes, seconds] = input.split(":").map(Number);
    if (isNaN(minutes) || isNaN(seconds)) return 0;
    return minutes * 60 + seconds;
};