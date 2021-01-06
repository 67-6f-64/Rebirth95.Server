skill_record_sql = ""


with open('rebirth_world0_rebirth_skill_list.sql', 'r') as iStream:
    with open('skill_records_output.sql', 'w') as oStream:
        
        for iLine in iStream:
            split1 = iLine.split('(')[2]
            
            split1 = split1[:-3]
            
            charid = split1[0:4]
            rest = split1[5:].strip()

            datasplit = rest.split(', ')

            newdata = []

            i = 0
            for data in datasplit:
                newdata.append(data.strip('\'').strip('{').strip('}'))
                i += 1

            skill_ids = newdata[0].split(',')
            skill_levels = newdata[1].split(',')
            expirations = newdata[2].split(',')
            skill_mastery = newdata[3].split(',')

            for i in range(len(skill_ids)):
                oStream.write(f"INSERT INTO rebirth.skill_records (charid, skillid, skill_level, skill_mastery, skill_expiration) VALUES ({charid}, {skill_ids[i]}, {skill_levels[i]}, {skill_mastery[i]}, null);\n")
