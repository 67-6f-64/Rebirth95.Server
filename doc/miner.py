from typing import Any


def RepresentsInt(s):
    try:
        int(s)
        return True
    except ValueError:
        return False


def get_prepared_statement(shopid, itemid, price, period, position):
    return "INSERT INTO rebirth.shopitems (shopid, itemid, price, itemperiod, position) VALUES ({},{},{},{},{});\n" \
        .format(shopid, itemid, price, period, position)


# shopId;itemId;price
def scrape_shopdata():
    istream = open("NpcShop.img.xml", "r")
    ostream = open("NpcShop_OutPut_SQL.sql", "w")

    istream.readline()
    istream.readline()

    curShopId = ""
    curItem = ""
    period = ""

    pos = 0

    while (True):
        line = istream.readline()
        if len(line) <= 0: break
        split = line.split('\"')
        if len(split) < 2: continue

        if RepresentsInt(split[1]) and int(split[1]) > 1000:
            curShopId = split[1]
            curItem = ""
            pos = 0
            continue

        if "item" in split[1]:
            curItem = split[3]
            continue

        if "period" in split[1]:
            period = split[3]
            continue

        if "price" in split[1]:
            curPrice = split[3]
            pos += 1
            ostream.write(get_prepared_statement(curShopId, curItem, curPrice, period, str(pos)))
            continue

        if "unitPrice" in split[1]:
            pos += 1
            ostream.write(get_prepared_statement(curShopId, curItem, "0", period, str(pos)))
            continue


def scrape_reward_data():
    istream = open("Reward.img.xml", "r")
    ostream = open("Reward_OutPut.txt", "w")

    mobId = ""

    while (True):
        line = istream.readline()
        if len(line) <= 0: break
        split = line.split('\"')
        if len(split) <= 2: continue

        if split[1][0] is "m" and not split[1] in ["money", "max", "min"]:
            mobId = split[1][1:]
            continue

        if "money" in split[1]:
            amount = split[3]
            prob = istream.readline().split('\"')[3][4:]
            ostream.write("{};money;{};{}\n".format(mobId, amount, prob))
            continue

        if "item" in split[1]:
            itemId = split[3]
            prob = istream.readline().split('\"')[3][4:]
            ostream.write("{};item;{};{}\n".format(mobId, itemId, prob))
            continue


def clean_npc_shop_data():
    istream = open("NpcShop_OutPut.txt", "r")
    istream2 = open("current_npc_shops.sql", "r")
    ostream = open("npc_shop_ids.txt", "w")

    npcs = []

    while True:
        line = istream2.readline()
        if len(line) <= 0: break
        split = line.split('(')
        item = split[2].split(',')[0]
        npcs.append(item)

    while True:
        line = istream.readline()
        if len(line) <= 0: break
        split = line.split(';')[0]
        if split in npcs: continue
        npcs.append(split)

    for npc in npcs:
        ostream.write("INSERT INTO rebirth.shops (shopid, npcid) VALUES ({}, {});\n".format(npc, npc))

    return npcs


def write_sql_shop_query():  # npcs):
    istream = open("NpcShop_OutPut.txt", "r")
    istream1 = open("npc_shop_ids.txt", "r")
    istream2 = open("current_npc_shops.sql", "r")
    ostream = open("npc_shop_items.txt", "w")

    # shopid, itemid, price, period, position
    entries = [[], [], [], [], []]

    index = 0
    curshop = ""

    while True:
        line = istream.readline()
        if len(line) <= 0: continue
        split = line.split(';')
        shopid = split[0]

        if curshop is not shopid: index = 0

        itemid = split[1]
        price = split[2]
        if len(itemid) <= 0 or len(price) <= 0: continue

        entries[0].append(shopid)
        entries[1].append(itemid)
        entries[2].append(price)
        entries[3].append(0)
        entries[4].append(index)
        index += 1

    for index in len(entries[0]):
        ostream.write("{},{},{},{},{}".format(entries[0], entries[1], entries[2], entries[3], entries[4], "\n"))
        index += 1


write_sql_shop_query()
