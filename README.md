# REhongo
An API for transliterating Japanese characters using http://nihongo.j-talk.com/

# Instructions
	// Create an instance of REHongo
	REHongo rehongo = new REHongo();

	// Either simply give it a string to transliterate
	string result = rehongo.Translate("日本語").Result; // output: 日本語(nihongo)

	// Or give it a custom packet
	REHongo_packet temp = new REHongo_packet("日本語", REHongo_packet.OUTPUT.HIRAGANA);
	result = rehongo.Translate(temp).Result; // output: 日本語(にほんご) 