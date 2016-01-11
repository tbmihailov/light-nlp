
rem run train with all modules
LightNlp.Demo data\sofia-signals-title-categorylevel1.txt "annotate_words,plain_bow,npref_2,npref_3,npref_4,nsuff_2,nsuff_3,nsuff_4,chngram_2,chngram_3,chngram_4,plain_word_stems,word2gram,word3gram, word4gram,count_punct,emoticons_dnevnikbg,doc_start,doc_end"

rem run train with some modules
LightNlp.Demo data\sofia-signals-title-categorylevel1.txt "annotate_words,plain_bow,nsuff_3,chngram_3,word2gram,doc_end"