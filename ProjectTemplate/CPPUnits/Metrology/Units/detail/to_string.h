#ifndef QTY_TO_STRING_H
#define QTY_TO_STRING_H

#include <string>
#include <vector>

namespace %NAMESPACE%
{
    /**
     * @brief Converts number to a string with unit of measure applied.
     * @tparam T - numeric type of the quantity,
     * @param quantity - pure number without a unit,
     * @param unit - unit string,
     * @param format - formatting string (something like "%f %s").
     * @return quantity string with unit applied.
     */
    template<typename T = double>
    std::string to_string(T quantity, const char* unit, const char* format)
    {
        std::size_t len = std::snprintf(nullptr, 0, format, quantity, unit);
        std::vector<char> buf(len + 1);
        std::snprintf(&buf[0], buf.size(), format, quantity, unit);
        return std::string(buf.cbegin(), buf.cend() - 1);
    }
}

#endif /* !QTY_TO_STRING_H */
