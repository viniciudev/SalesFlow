import CharacterRemover from 'character-remover';


export const ConvertToUSD = (text) => {
    if (isNaN(text)) {
        const chars = ['R', '$', '.']
        let val = CharacterRemover.removeOnly(text, chars)
        val= val.replace(",",".")

        return val
    }
    return text
}

export default ConvertToUSD