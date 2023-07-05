namespace PSIDtoBas;

class Program{
    //Enum for PSID file offsets
    enum PSID {
        magic = 0,  //MagicID "PSID" 4 bytes
        version = 4,  //version 2 byes
        data_offset = 6,  //data offset of music code 2 bytes
        load_addr = 8,  //load address of music code 2 bytes
        init_addr = 10,  //init address of music code 2 bytes
        play_addr = 12,  //play address of music code 2 bytes
        number_of_songs = 14,  //number of songs 2 bytes
        default_song = 16,  //index of default song 2 bytes
        speed = 18,  //bitmask to set speed using CIA(1) or raster int(0) 4 bytes
        name = 22, //Name of music as null term string, 32 bytes
        author = 54, //Author of music as null term string, 32 bytes
        released = 86  //Release information as null term string, 32 bytes
    }

    static void buildFor(ushort addr, int musicLength) {
        Console.WriteLine($"10 for i={addr.ToString()} to {addr.ToString()}+{musicLength.ToString()}:read a:poke i,a:next i : rem load music");
    }

    static void buildSys(ushort init, ushort play, int idx) {
        Console.WriteLine($"{(idx+1).ToString()} sys {init.ToString()} : rem init music");
        Console.WriteLine($"{(idx+2).ToString()} sys {play.ToString()} : rem play music");
    }

    static int buildData(byte[] music, ushort offset, int basLine) {
        Console.Write($"{basLine.ToString()} rem music code");
        for(ushort i = 0; i < music.Length - offset; i++) {
            if(i % 12 == 0) {
                basLine++;
                Console.WriteLine();
                Console.Write($"{basLine.ToString()} data ");
            }
            Console.Write(music[i].ToString() + ((i+1)%12 != 0 ? "," : ""));
        }
        Console.WriteLine();
        return basLine;
    }

    static ushort read2bytes(byte[] bytes, int offset) {
        //C64 bytes have different endian and need swapped
        var bytestor = new byte[2];
        bytestor[1] = bytes[offset];
        bytestor[0] = bytes[offset+1];
        return BitConverter.ToUInt16(bytestor);
    }

    static void Main(string[] args) {
        //Get name of file to convert
        if(args.Count() < 1) {
            Console.WriteLine("PSIDtoBas <PSIDFilename>");
            Environment.Exit(1);
        }

        //Load file to byte array
        var ba = File.ReadAllBytes(args[0]);
        //Extract addresses and length
        ushort dataOffset = read2bytes(ba,(int)PSID.data_offset);
        ushort loadAddr = read2bytes(ba,(int)PSID.load_addr);
        if(loadAddr == 0) {
            loadAddr = BitConverter.ToUInt16(ba,dataOffset);
            dataOffset++;
            dataOffset++;
        }
        ushort initAddr = read2bytes(ba,(int)PSID.init_addr);
        ushort playAddr = read2bytes(ba,(int)PSID.play_addr); 
        int dataLength = ba.Length - dataOffset;
        int dataLineNo = 11;
 
        //Build For loop
        buildFor(loadAddr,dataLength);
        //Build Data
        dataLineNo = buildData(ba, dataOffset, dataLineNo);
        //Build Sys calls
        buildSys(initAddr,playAddr,dataLineNo);
    }
}
