using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KPT.Parser
{
    static class Constants
    {
        public const int NAME_LENGTH = 0x14;
        public const int CHOICE_STRING_LENGTH = 0x38;
        public const int OPCODE_SIZE = 0x02;
        public static Dictionary<string, string> TRANSLATED_NAME_TAGS = new Dictionary<string, string>()
        {
            {"太一","Taichi"},
            {"稲葉", "Inaba" },
            {"伊織", "Iori" },
            {"唯", "Yui" },
            {"青木", "Aoki" },
            {"藤島", "Fujishima" },
            {"平田先生", "Ms Hirata"},
            {"後藤", "Mr. Gotou"},
            {"〈ふうせんかずら〉", "〈Heartseed〉"},
            {"中山", "Nakayama"},
            {"雪菜", "Yukina"},
            {"伊織の元父", "Iori's Bio Dad"},
            {"伊織の母", "Iori's Mom"},
            {"大沢", "Oosawa"},
            {"不良Ｄ", "Delinquent D"},
            {"莉奈", "Rina"},
            {"渡瀬", "Watase"},
            {"４人", "All Four" },
            {"伊織＆唯＆青木", "Iori＆Yui＆Aoki"},
            {"一同 -", "All"},
            {"化学教師", "Chem Teacher"},
            {"瀬戸内", "Setouchi"},
            {"３人", "All Three" },
            {"太一・唯・伊織", "Taichi・Yui・Iori"},
            {"太一＆稲葉", "Taichi＆Inaba"},
            {"用務員", "Janitor"},
            {"太一＆伊織", "Taichi＆Iori"},
            {"太一＆青木＆唯", "Taichi＆Aoki＆Yui"},
            {"女子生徒", "Female Classmate"},
            {"男子生徒", "Male Classmate"},
            {"青木＆大沢", "Aoki＆Oosawa"},
            {"教師", "Teacher"},
            {"不良Ａ", "Delinquent Ａ"},
            {"姫子", "Himeko"},
            {"女子一同", "All The Girls"},
            {"稲葉以外の女子", "Girls Except Inaba"},
            {"女性陣", "Female Group"},
            {"太一＆青木", "Taichi＆Aoki"},
            {"伊織＆唯", "Iori＆Yui"},
            {"唯＆伊織", "Yui＆Iori"},
            {"稲葉＆唯",  "Inaba＆Yui"},
            {"唯＆青木", "Yui＆Aoki"},
            {"一同", "All"},
            {"不良Ｂ", "Delinquent Ｂ"},
            {"不良Ｃ", "Delinquent Ｃ"},
            {"警官", "Police Officer"},
            {"不良", "Delinquent"},
            {"女の子", "Girl"},
        };
    }
}
