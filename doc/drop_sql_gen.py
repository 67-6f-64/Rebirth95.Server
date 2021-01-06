# quick n dirty script to generate sql drop insert queries

drops = {}
s = "INSERT INTO rebirth_world0.rebirth.drop_data (dropperid, itemid, minimum_quantity, maximum_quantity, questid, chance)"

print("enter mob ID to drop items: ")
mobId = input()

print(f"enter item ID and odds separated by a space, one drop at a time. end input with 0.")
print(f"to change mob ID, enter mob ID, item ID, and odds separated by space")

while True:
    userinput = input()

    if len(userinput) <= 0 or int(userinput.split(' ')[0]) <= 0:
        break

    userinput = userinput.split(' ')

    if len(userinput) == 3:
        mobId = userinput[0]
    elif len(userinput) != 2:
        print("input length must be two integers separated by a space")
        continue

    s += f"\r\nVALUES ({mobId}, {userinput[len(userinput) - 2]}, 1, 1, 0, {userinput[len(userinput) - 1]}),"

s = s[:-1]
s += ";"

print(s)

