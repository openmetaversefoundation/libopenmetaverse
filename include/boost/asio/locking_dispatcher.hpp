//
// locking_dispatcher.hpp
// ~~~~~~~~~~~~~~~~~~~~~~
//
// Copyright (c) 2003-2005 Christopher M. Kohlhoff (chris at kohlhoff dot com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
//

#ifndef BOOST_ASIO_LOCKING_DISPATCHER_HPP
#define BOOST_ASIO_LOCKING_DISPATCHER_HPP

#if defined(_MSC_VER) && (_MSC_VER >= 1200)
# pragma once
#endif // defined(_MSC_VER) && (_MSC_VER >= 1200)

#include <boost/asio/detail/push_options.hpp>

#include <boost/asio/basic_locking_dispatcher.hpp>
#include <boost/asio/locking_dispatcher_service.hpp>

namespace boost {
namespace asio {

/// Typedef for the typical usage of locking_dispatcher.
typedef basic_locking_dispatcher<locking_dispatcher_service<> >
  locking_dispatcher;

} // namespace asio
} // namespace boost

#include <boost/asio/detail/pop_options.hpp>

#endif // BOOST_ASIO_LOCKING_DISPATCHER_HPP
